namespace StajFlow.Api.Services.Interfaces;

public interface ILlmQuotaService
{
    Task<LlmQuotaReservation> ReserveAsync(CancellationToken cancellationToken);
    Task RecordTokensAsync(DateOnly date, int inputTokens, int outputTokens, CancellationToken cancellationToken);
    Task BlockAsync(DateTimeOffset until, CancellationToken cancellationToken);
}

public record LlmQuotaReservation(bool Allowed, DateOnly Date, DateTimeOffset? RetryAt = null);
