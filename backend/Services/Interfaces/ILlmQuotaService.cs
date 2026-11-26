namespace StajFlow.Api.Services.Interfaces;

public interface ILlmQuotaService
{
    Task<LlmQuotaReservation> ReserveAsync(LlmTokenBudget tokens, CancellationToken cancellationToken);
    Task RecordTokensAsync(LlmQuotaReservation reservation, int inputTokens, int outputTokens, CancellationToken cancellationToken);
    Task BlockAsync(DateTimeOffset until, CancellationToken cancellationToken);
}

public record LlmTokenBudget(int InputTokens, int OutputTokens)
{
    public long Total => (long)InputTokens + OutputTokens;
}

public record LlmQuotaReservation(bool Allowed, DateOnly Date, DateTimeOffset? RetryAt = null, long ReservedTokens = 0);
