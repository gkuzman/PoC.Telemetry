namespace Shared.Contracts;

/// <summary>
/// Event published when a withdrawal request is submitted via the API.
/// Consumers should treat this as an immutable fact — do not mutate after publishing.
/// </summary>
/// <param name="AccountId">The account identifier the message relates to.</param>
/// <param name="WithdrawalId">The withdrawal identifier</param>
/// <param name="Amount">The amount to withdraw</param>
/// <param name="OccurredAt">UTC timestamp when the event was raised.</param>
/// <param name="CorrelationId">Distributed tracing / correlation identifier.</param>
public sealed record InitiateWithdrawalMessage(
    int AccountId,
    int WithdrawalId,
    decimal Amount,
    DateTimeOffset OccurredAt,
    string CorrelationId);

