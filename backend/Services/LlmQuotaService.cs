using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StajFlow.Api.Configurations;
using StajFlow.Api.Data;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;

namespace StajFlow.Api.Services;

public sealed class LlmQuotaService(IServiceScopeFactory scopeFactory, IOptions<GroqOptions> options, TimeProvider clock) : ILlmQuotaService
{
    private const string Provider = "Groq";

    public static DateOnly GetQuotaDate(DateTimeOffset time) => DateOnly.FromDateTime(time.UtcDateTime);

    public static DateTimeOffset GetNextQuotaDay(DateTimeOffset time)
    {
        return new DateTimeOffset(GetQuotaDate(time).AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
    }

    public Task<LlmQuotaReservation> ReserveAsync(LlmTokenBudget tokens, CancellationToken cancellationToken) =>
        WithBudgetAsync((budget, now) =>
        {
            if (tokens.InputTokens < 0 || tokens.OutputTokens < 1 || tokens.Total > options.Value.TokensPerMinute ||
                budget.RequestCount >= options.Value.DailyRequestLimit || tokens.Total > options.Value.DailyTokenLimit - budget.ReservedTokens)
                return new LlmQuotaReservation(false, budget.Date);
            if (budget.BlockedUntil > now)
                return new(false, budget.Date, budget.BlockedUntil <= now.AddSeconds(options.Value.MaxRetryDelaySeconds) ? budget.BlockedUntil : null);
            if (now >= budget.WindowStartedAt.AddMinutes(1))
            {
                budget.WindowStartedAt = now;
                budget.WindowRequestCount = 0;
                budget.WindowReservedTokens = 0;
            }
            if (budget.WindowRequestCount >= options.Value.RequestsPerMinute ||
                tokens.Total > options.Value.TokensPerMinute - budget.WindowReservedTokens)
                return new(false, budget.Date, budget.WindowStartedAt.AddMinutes(1));
            budget.RequestCount++;
            budget.WindowRequestCount++;
            budget.ReservedTokens += tokens.Total;
            budget.WindowReservedTokens += tokens.Total;
            return new(true, budget.Date, ReservedTokens: tokens.Total);
        }, cancellationToken);

    public async Task RecordTokensAsync(LlmQuotaReservation reservation, int inputTokens, int outputTokens, CancellationToken cancellationToken) =>
        await WithBudgetAsync((budget, _) =>
        {
            budget.InputTokens += Math.Max(0, inputTokens);
            budget.OutputTokens += Math.Max(0, outputTokens);
            budget.ReservedTokens = Math.Max(0, budget.ReservedTokens - reservation.ReservedTokens) +
                Math.Max(0L, inputTokens) + Math.Max(0L, outputTokens);
            return true;
        }, cancellationToken, reservation.Date);

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
        var budget = await context.LlmDailyUsage.SingleOrDefaultAsync(record => record.Provider == Provider && record.Date == day, cancellationToken);
        if (budget is null)
        {
            var previous = await context.LlmDailyUsage.AsNoTracking().Where(record => record.Provider == Provider && record.Date < day)
                .OrderByDescending(record => record.Date).FirstOrDefaultAsync(cancellationToken);
            budget = new LlmDailyUsage { Provider = Provider, Date = day, WindowStartedAt = now };
            if (previous is not null)
            {
                if (previous.WindowStartedAt.AddMinutes(1) > now)
                {
                    budget.WindowStartedAt = previous.WindowStartedAt;
                    budget.WindowRequestCount = previous.WindowRequestCount;
                    budget.WindowReservedTokens = previous.WindowReservedTokens;
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
