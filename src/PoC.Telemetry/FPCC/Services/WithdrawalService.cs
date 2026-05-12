using System.Diagnostics;
using FPCC.Requests;
using Shared;

namespace FPCC.Services;

public class WithdrawalService : IWithdrawalService
{
    public async Task Process(InitiateWithdrawalRequest request)
    {
        if (request.Amount == 1)
        {
            await BasicExample(request);
        }
        else if (request.Amount == 2)
        {
            await BasicSpanExample(request);
        }
        else if (request.Amount == 3)
        {
            await BasicNewRootSpanExample(request);
        }
        else if (request.Amount == 4)
        {
            await AdvancedSpanExample(request);
        }
        else if (request.Amount == 5)
        {
            await AdvancedSpanNoStopExample(request);
        }
        else if (request.Amount == 6)
        {
            await AdvancedNewRootSpanExample(request);
        }
    }

    private async Task AdvancedNewRootSpanExample(InitiateWithdrawalRequest request)
    {
        using var activity = request.StartNewRootSpanFromRequest(TracingExtensions.Source);
        activity.SetTag("fpcc.withdrawal.id", request.WithdrawalId);
        activity.SetTag("fpcc.withdrawal.amount", request.Amount);
        activity.SetTag("fpcc.withdrawal.account_id", request.AccountId);
        await Task.Delay(1000);

        // mimic getting fraud force
        using var activity2 = TracingExtensions.Source.StartActivity("Get FraudForce");
        activity2.SetTag("fpcc.fraudforce.score", -3);
        await Task.Delay(5000);
    }

    private async Task AdvancedSpanExample(InitiateWithdrawalRequest request)
    {
        using var activity = request.StartNewSpanFromRequest(TracingExtensions.Source);
        activity.SetTag("fpcc.withdrawal.id", request.WithdrawalId);
        activity.SetTag("fpcc.withdrawal.amount", request.Amount);
        activity.SetTag("fpcc.withdrawal.account_id", request.AccountId);
        await Task.Delay(1000);
        activity.Stop();

        // mimic getting fraud force
        using var activity2 = TracingExtensions.Source.StartActivity("Get FraudForce");
        activity2.SetTag("fpcc.fraudforce.score", -3);
        await Task.Delay(5000);
    }

    private async Task AdvancedSpanNoStopExample(InitiateWithdrawalRequest request)
    {
        using var activity = request.StartNewSpanFromRequest(TracingExtensions.Source);
        activity.SetTag("fpcc.withdrawal.id", request.WithdrawalId);
        activity.SetTag("fpcc.withdrawal.amount", request.Amount);
        activity.SetTag("fpcc.withdrawal.account_id", request.AccountId);
        await Task.Delay(1000);

        // mimic getting fraud force
        using var activity2 = TracingExtensions.Source.StartActivity("Get FraudForce");
        activity2.SetTag("fpcc.fraudforce.score", -3);
        await Task.Delay(5000);
    }

    private async Task BasicNewRootSpanExample(InitiateWithdrawalRequest request)
    {
        using var activity = request.StartNewRootSpanFromRequest(TracingExtensions.Source);
        activity.SetTag("fpcc.withdrawal.id", request.WithdrawalId);
        activity.SetTag("fpcc.withdrawal.amount", request.Amount);
        activity.SetTag("fpcc.withdrawal.account_id", request.AccountId);
        await Task.Delay(1000);

        // mimic getting fraud force
        activity.SetTag("fpcc.fraudforce.score", -3);
        await Task.Delay(5000);
    }

    private async Task BasicSpanExample(InitiateWithdrawalRequest request)
    {
        using var activity = request.StartNewSpanFromRequest(TracingExtensions.Source);
        activity.SetTag("fpcc.withdrawal.id", request.WithdrawalId);
        activity.SetTag("fpcc.withdrawal.amount", request.Amount);
        activity.SetTag("fpcc.withdrawal.account_id", request.AccountId);
        await Task.Delay(3000);

        // mimic getting fraud force
        activity.SetTag("fpcc.fraudforce.score", -3);
        await Task.Delay(5000);
    }

    private async Task BasicExample(InitiateWithdrawalRequest request)
    {
        var activity = Activity.Current;
        activity.SetTag("fpcc.withdrawal.id", request.WithdrawalId);
        activity.SetTag("fpcc.withdrawal.amount", request.Amount);
        activity.SetTag("fpcc.withdrawal.account_id", request.AccountId);
        await Task.Delay(3000);

        // mimic getting fraud force
        activity.SetTag("fpcc.fraudforce.score", -3);
        await Task.Delay(5000);
    }
}