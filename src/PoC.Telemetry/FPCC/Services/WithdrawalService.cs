using System.Diagnostics;
using System.Net.Http.Json;
using FPCC.Requests;
using Shared;
using Shared.Models;

namespace FPCC.Services;

public class WithdrawalService(IHttpClientFactory httpClientFactory) : IWithdrawalService
{
    public async Task Process(InitiateWithdrawalRequest request)
    {
        var sw = Stopwatch.StartNew();
        var status = "success";
        try
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
            else if (request.Amount == 7)
            {
                await SlowExample(request);
            }
            else if (request.Amount == 8)
            {
                await ExceptionExample(request);
            }
            else if (request.Amount == 9)
            {
                await ExceptionPamExample(request);
            }
        }
        catch
        {
            status = "failure";
            throw;
        }
        finally
        {
            sw.Stop();
            FpccMetrics.FpccWithdrawalProcessed.Add(1,
                new KeyValuePair<string, object?>("status", status));
            FpccMetrics.FpccWithdrawalProcessingDuration.Record(sw.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("status", status));
        }
    }

    private async Task ExceptionPamExample(InitiateWithdrawalRequest request)
    {
        using var activity = request.StartNewSpanFromRequest(TracingExtensions.Source);
        try
        {
            activity.SetTag(FpccAttributes.FpccWithdrawalId, request.WithdrawalId);
            activity.SetTag(FpccAttributes.FpccWithdrawalAmount, request.Amount);
            activity.SetTag(FpccAttributes.FpccWithdrawalAccountId, request.AccountId);
            await Task.Delay(1000);
            activity.Stop();

            // mimic getting fraud force
            var fraudSw = Stopwatch.StartNew();
            using var activity2 = TracingExtensions.Source.StartActivity("Get FraudForce");
            const int fraudScore = -3;
            activity2.SetTag(FpccAttributes.FpccFraudforceScore, fraudScore);
            await Task.Delay(2000);
            fraudSw.Stop();
            FpccMetrics.FpccFraudforceDuration.Record(fraudSw.Elapsed.TotalMilliseconds);
            FpccMetrics.FpccFraudforceScore.Record(fraudScore);

            await ConfirmWithdrawalAsync(request, activity.Context);
        }
        catch (Exception e)
        {
            activity.AddException(e);
            throw;
        }
    }

    private async Task ExceptionExample(InitiateWithdrawalRequest request)
    {
        using var activity = request.StartNewSpanFromRequest(TracingExtensions.Source);
        try
        {
            activity.SetTag(FpccAttributes.FpccWithdrawalId, request.WithdrawalId);
            activity.SetTag(FpccAttributes.FpccWithdrawalAmount, request.Amount);
            activity.SetTag(FpccAttributes.FpccWithdrawalAccountId, request.AccountId);
            await Task.Delay(1000);
            activity.Stop();

            // mimic getting fraud force
            var fraudSw = Stopwatch.StartNew();
            using var activity2 = TracingExtensions.Source.StartActivity("Get FraudForce");
            await Task.Delay(1000);
            fraudSw.Stop();
            FpccMetrics.FpccFraudforceDuration.Record(fraudSw.Elapsed.TotalMilliseconds);

            throw new Exception("FraudForce not available");

            await ConfirmWithdrawalAsync(request, activity.Context);
        }
        catch (Exception e)
        {
           activity.AddException(e);
           FpccMetrics.FpccWithdrawalProcessingError.Add(1);
           throw;
        }
    }

    private async Task SlowExample(InitiateWithdrawalRequest request)
    {
        using var activity = request.StartNewSpanFromRequest(TracingExtensions.Source);
        activity.SetTag(FpccAttributes.FpccWithdrawalId, request.WithdrawalId);
        activity.SetTag(FpccAttributes.FpccWithdrawalAmount, request.Amount);
        activity.SetTag(FpccAttributes.FpccWithdrawalAccountId, request.AccountId);
        await Task.Delay(1000);
        activity.Stop();

        // mimic getting fraud force
        var fraudSw = Stopwatch.StartNew();
        using var activity2 = TracingExtensions.Source.StartActivity("Get FraudForce");
        const int fraudScore = -3;
        activity2.SetTag(FpccAttributes.FpccFraudforceScore, fraudScore);
        await Task.Delay(7000);
        fraudSw.Stop();
        FpccMetrics.FpccFraudforceDuration.Record(fraudSw.Elapsed.TotalMilliseconds);
        FpccMetrics.FpccFraudforceScore.Record(fraudScore);

        await ConfirmWithdrawalAsync(request, activity.Context);
    }

    private async Task AdvancedNewRootSpanExample(InitiateWithdrawalRequest request)
    {
        using var activity = request.StartNewRootSpanFromRequest(TracingExtensions.Source);
        activity.SetTag(FpccAttributes.FpccWithdrawalId, request.WithdrawalId);
        activity.SetTag(FpccAttributes.FpccWithdrawalAmount, request.Amount);
        activity.SetTag(FpccAttributes.FpccWithdrawalAccountId, request.AccountId);
        activity.SetTag(FpccAttributes.FpccWithdrawalIban, "NL18RABO0123459876");
        await Task.Delay(1000);

        // mimic getting fraud force
        var fraudSw = Stopwatch.StartNew();
        using var activity2 = TracingExtensions.Source.StartActivity("Get FraudForce", ActivityKind.Internal, activity.Context);
        const int fraudScore = -3;
        activity2.SetTag(FpccAttributes.FpccFraudforceScore, fraudScore);
        await Task.Delay(2000);
        fraudSw.Stop();
        FpccMetrics.FpccFraudforceDuration.Record(fraudSw.Elapsed.TotalMilliseconds);
        FpccMetrics.FpccFraudforceScore.Record(fraudScore);

        await ConfirmWithdrawalAsync(request, activity.Context);
    }

    private async Task AdvancedSpanExample(InitiateWithdrawalRequest request)
    {
        using var activity = request.StartNewSpanFromRequest(TracingExtensions.Source);
        activity.SetTag(FpccAttributes.FpccWithdrawalId, request.WithdrawalId);
        activity.SetTag(FpccAttributes.FpccWithdrawalAmount, request.Amount);
        activity.SetTag(FpccAttributes.FpccWithdrawalAccountId, request.AccountId);
        await Task.Delay(1000);
        activity.Stop();

        // mimic getting fraud force
        var fraudSw = Stopwatch.StartNew();
        using var activity2 = TracingExtensions.Source.StartActivity("Get FraudForce");
        const int fraudScore = -3;
        activity2.SetTag(FpccAttributes.FpccFraudforceScore, fraudScore);
        await Task.Delay(2000);
        fraudSw.Stop();
        FpccMetrics.FpccFraudforceDuration.Record(fraudSw.Elapsed.TotalMilliseconds);
        FpccMetrics.FpccFraudforceScore.Record(fraudScore);

        await ConfirmWithdrawalAsync(request, activity.Context);
    }

    private async Task AdvancedSpanNoStopExample(InitiateWithdrawalRequest request)
    {
        using var activity = request.StartNewSpanFromRequest(TracingExtensions.Source);
        activity.SetTag(FpccAttributes.FpccWithdrawalId, request.WithdrawalId);
        activity.SetTag(FpccAttributes.FpccWithdrawalAmount, request.Amount);
        activity.SetTag(FpccAttributes.FpccWithdrawalAccountId, request.AccountId);
        await Task.Delay(1000);

        // mimic getting fraud force
        var fraudSw = Stopwatch.StartNew();
        using var activity2 = TracingExtensions.Source.StartActivity("Get FraudForce", ActivityKind.Internal, activity.Context);
        const int fraudScore = -3;
        activity2.SetTag(FpccAttributes.FpccFraudforceScore, fraudScore);
        await Task.Delay(2000);
        fraudSw.Stop();
        FpccMetrics.FpccFraudforceDuration.Record(fraudSw.Elapsed.TotalMilliseconds);
        FpccMetrics.FpccFraudforceScore.Record(fraudScore);

        await ConfirmWithdrawalAsync(request, activity.Context);
    }

    private async Task BasicNewRootSpanExample(InitiateWithdrawalRequest request)
    {
        using var activity = request.StartNewRootSpanFromRequest(TracingExtensions.Source);
        activity.SetTag(FpccAttributes.FpccWithdrawalId, request.WithdrawalId);
        activity.SetTag(FpccAttributes.FpccWithdrawalAmount, request.Amount);
        activity.SetTag(FpccAttributes.FpccWithdrawalAccountId, request.AccountId);
        await Task.Delay(1000);

        // mimic getting fraud force
        activity.SetTag(FpccAttributes.FpccFraudforceScore, -3);
        await Task.Delay(2000);
    }

    private async Task BasicSpanExample(InitiateWithdrawalRequest request)
    {
        using var activity = request.StartNewSpanFromRequest(TracingExtensions.Source);
        activity.SetTag(FpccAttributes.FpccWithdrawalId, request.WithdrawalId);
        activity.SetTag(FpccAttributes.FpccWithdrawalAmount, request.Amount);
        activity.SetTag(FpccAttributes.FpccWithdrawalAccountId, request.AccountId);
        await Task.Delay(1000);

        // mimic getting fraud force
        activity.SetTag(FpccAttributes.FpccFraudforceScore, -3);
        await Task.Delay(2000);
    }

    private async Task BasicExample(InitiateWithdrawalRequest request)
    {
        var activity = Activity.Current;
        activity.SetTag(FpccAttributes.FpccWithdrawalId, request.WithdrawalId);
        activity.SetTag(FpccAttributes.FpccWithdrawalAmount, request.Amount);
        activity.SetTag(FpccAttributes.FpccWithdrawalAccountId, request.AccountId);
        await Task.Delay(1000);

        // mimic getting fraud force
        activity.SetTag(FpccAttributes.FpccFraudforceScore, -3);
        await Task.Delay(2000);
    }

    private async Task ConfirmWithdrawalAsync(InitiateWithdrawalRequest request, ActivityContext activityContext)
    {
        var sw = Stopwatch.StartNew();
        using var activity = TracingExtensions.Source.StartActivity("ConfirmWithdrawal", ActivityKind.Client, activityContext);
        activity?.SetTag(FpccAttributes.FpccWithdrawalId, request.WithdrawalId);
        activity?.SetTag(FpccAttributes.FpccWithdrawalAccountId, request.AccountId);

        try
        {
            var client = httpClientFactory.CreateClient("PAM");
            var payload = new ConfirmWithdrawal(request.WithdrawalId, request.AccountId);
            var response = await client.PostAsJsonAsync("api/Withdrawals/Confirm", payload);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            FpccMetrics.FpccWithdrawalConfirmError.Add(1);
            throw;
        }
        finally
        {
            sw.Stop();
            FpccMetrics.FpccWithdrawalConfirmDuration.Record(sw.Elapsed.TotalMilliseconds);
        }
    }
}
