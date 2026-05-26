namespace Shared.Messaging;

internal interface IAzureServiceBusMessageHandler
{
    string GetName();
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}