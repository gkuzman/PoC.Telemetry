using System.Diagnostics;

namespace Shared.Messaging;

public static class ServiceBusDiagnosticSettings
{
    public static ActivitySource Source = new ActivitySource("ServiceBus");
}