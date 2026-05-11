namespace Shared.Messaging;

internal interface IAzureServiceBusMessageHandler
{
    string GetName();
    Task ManageMessageHandlerStatus(CancellationToken cancellationToken);
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}