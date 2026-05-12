using Shared;
using Shared.Contracts;

namespace FPCC.Requests;

public class InitiateWithdrawalRequest(InitiateWithdrawalMessage message) : TraceableRequest
{
    public int AccountId { get; } = message.AccountId;
    public int WithdrawalId { get; } = message.WithdrawalId;
    public decimal Amount { get; } = message.Amount;
    public DateTimeOffset OccurredAt { get; } = message.OccurredAt;
}