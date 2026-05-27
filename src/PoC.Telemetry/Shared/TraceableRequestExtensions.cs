using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Shared;

public static class TraceableRequestExtensions
{
    public static ActivitySource Source = new("Shared.RequestTracing");
    public static void AddCurrentTraceContext(this TraceableRequest request)
    {
        Propagators.DefaultTextMapPropagator.Inject(
            new PropagationContext(Activity.Current?.Context ?? new ActivityContext(), Baggage.Current),
            request.RequestHeaders, (headers, key, value) => headers[key] = [value]);
    }

    public static Activity? StartNewSpanFromRequest(this TraceableRequest request, ActivitySource? source = null)
    {
        var context = Propagators.DefaultTextMapPropagator.Extract(
            new PropagationContext(Activity.Current?.Context ?? new ActivityContext(), Baggage.Current),
            request.RequestHeaders, (headers, key) => headers.TryGetValue(key, out var value) ? value : null);

        Baggage.Current = context.Baggage;

        source ??= Source;

        return source.StartActivity($"Process {request.GetType().Name}",
            ActivityKind.Internal,
            context.ActivityContext);
    }

    public static Activity? StartNewRootSpanFromRequest(this TraceableRequest request,  ActivitySource? source = null)
    {
        // Capture the consumer span before clearing Activity.Current,
        // so we can establish bidirectional links between the two traces.
        var consumerActivity = Activity.Current;
        var linkedContext = consumerActivity?.Context;

        Baggage.Current = Baggage.Current; // preserve baggage across the root boundary
        Activity.Current = null;

        source ??= Source;

        var links = linkedContext.HasValue
            ? new[] { new ActivityLink(linkedContext.Value) }
            : Array.Empty<ActivityLink>();

        var newRootActivity = source.StartActivity(
            $"Process {request.GetType().Name}",
            ActivityKind.Server,
            parentContext: default,
            links: links);

        // Add forward link: consumer → new root, so Honeycomb can navigate
        // from the original trace to the new root trace (bidirectional).
        // Requires .NET 9+ (Activity.AddLink API).
        if (newRootActivity is not null && consumerActivity is not null)
        {
            consumerActivity.AddLink(new ActivityLink(newRootActivity.Context));
        }

        return newRootActivity;
    }
}

public abstract class TraceableRequest
{
    public TraceableRequest()
    {
        this.AddCurrentTraceContext();
    }
    public Dictionary<string, string[]> RequestHeaders { get; set; } = [];
}