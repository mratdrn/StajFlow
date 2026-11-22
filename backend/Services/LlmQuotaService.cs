using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StajFlow.Api.Configurations;
using StajFlow.Api.Data;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;

namespace StajFlow.Api.Services;

public sealed class LlmQuotaService(IServiceScopeFactory scopeFactory, IOptions<GeminiOptions> options, TimeProvider clock) : ILlmQuotaService
{
    private static readonly TimeZoneInfo QuotaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");

    public static DateOnly GetQuotaDate(DateTimeOffset time) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(time, QuotaTimeZone).DateTime);

    public static DateTimeOffset GetNextQuotaDay(DateTimeOffset time)
    {
        var midnight = GetQuotaDate(time).AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(midnight, QuotaTimeZone));
    }

    public Task<LlmQuotaReservation> ReserveAsync(CancellationToken cancellationToken) =>
        WithBudgetAsync((budget, now) =>
        {
            if (budget.RequestCount >= options.Value.DailyRequestLimit)
                return new LlmQuotaReservation(false, budget.Date);
            if (budget.BlockedUntil > now)
                return new(false, budget.Date, budget.BlockedUntil <= now.AddSeconds(options.Value.MaxRetryDelaySeconds) ? budget.BlockedUntil : null);
            if (now >= budget.WindowStartedAt.AddMinutes(1))
            {
                budget.WindowStartedAt = now;
                budget.WindowRequestCount = 0;
            }
            if (budget.WindowRequestCount >= options.Value.RequestsPerMinute)
                return new(false, budget.Date, budget.WindowStartedAt.AddMinutes(1));
            budget.RequestCount++;
            budget.WindowRequestCount++;
            return new(true, budget.Date);
        }, cancellationToken);

    public async Task RecordTokensAsync(DateOnly date, int inputTokens, int outputTokens, CancellationToken cancellationToken) =>
        await WithBudgetAsync((budget, _) =>
        {
            budget.InputTokens += Math.Max(0, inputTokens);
            budget.OutputTokens += Math.Max(0, outputTokens);
            return true;
        }, cancellationToken, date);

    public async Task BlockAsync(DateTimeOffset until, CancellationToken cancellationToken) =>
        await WithBudgetAsync((budget, _) =>
        {
            if (budget.BlockedUntil is null || budget.BlockedUntil < until) budget.BlockedUntil = until;
            return true;
        }, cancellationToken);

    private async Task<T> WithBudgetAsync<T>(Func<LlmDailyUsage, DateTimeOffset, T> action, CancellationToken cancellationToken, DateOnly? date = null)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await context.Database.ExecuteSqlRawAsync("""
            DECLARE @result int;
            EXEC @result = sp_getapplock @Resource = N'StajFlow.LlmQuota', @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 5000;
            IF @result < 0 THROW 51000, N'Kota kilidi alınamadı.', 1;
            """, cancellationToken);
        var now = clock.GetUtcNow();
        var day = date ?? GetQuotaDate(now);
        var budget = await context.LlmDailyUsage.SingleOrDefaultAsync(record => record.Date == day, cancellationToken);
        if (budget is null)
        {
            var previous = await context.LlmDailyUsage.AsNoTracking().Where(record => record.Date < day)
                .OrderByDescending(record => record.Date).FirstOrDefaultAsync(cancellationToken);
            budget = new LlmDailyUsage { Date = day, WindowStartedAt = now };
            if (previous is not null)
            {
                if (previous.WindowStartedAt.AddMinutes(1) > now)
                {
                    budget.WindowStartedAt = previous.WindowStartedAt;
                    budget.WindowRequestCount = previous.WindowRequestCount;
                }
                if (previous.BlockedUntil > now) budget.BlockedUntil = previous.BlockedUntil;
            }
            context.LlmDailyUsage.Add(budget);
        }
        var result = action(budget, now);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }
}
