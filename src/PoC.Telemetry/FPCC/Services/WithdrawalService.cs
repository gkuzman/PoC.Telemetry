using System.Diagnostics;
using FPCC.Requests;
using Shared;

namespace FPCC.Services;

public class WithdrawalService : IWithdrawalService
{
    public async Task Process(InitiateWithdrawalRequest request)
    {
        Activity? activity = null;
        activity = request.Amount == 1 ? request.StartNewSpanFromRequest() : request.StartNewRootSpanFromRequest();
        activity.SetTag("fpcc.withdrawal.id", request.WithdrawalId);
        activity.SetTag("fpcc.withdrawal.amount", request.Amount);
        activity.SetTag("fpcc.withdrawal.account_id", request.AccountId);

        await Task.Delay(5000);
    }
}