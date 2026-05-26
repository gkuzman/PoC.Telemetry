using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.Messaging;

internal class AzureServiceBusHostedService : BackgroundService
{
    private readonly IEnumerable<IAzureServiceBusMessageHandler> _messageHandlers;
    private readonly ILogger<AzureServiceBusHostedService> _logger;

    public AzureServiceBusHostedService(
        IEnumerable<IAzureServiceBusMessageHandler> messageHandlers,
        ILogger<AzureServiceBusHostedService> logger)
    {
        _messageHandlers = messageHandlers;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await StartServiceBusMessageHandlers(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown — expected when the host is stopping.
            _logger.LogInformation("Azure Service Bus hosted service is stopping.");
        }
        finally
        {
            await StopServiceBusMessageHandlers(CancellationToken.None);
        }
    }

    private async Task StartServiceBusMessageHandlers(CancellationToken cancellationToken)
    {
        foreach (var messageHandler in _messageHandlers)
        {
            try
            {
                await messageHandler.StartAsync(cancellationToken);
                _logger.LogInformation("Started Service Bus message handler for {HandlerName}.", messageHandler.GetName());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to start the {HandlerName} Service Bus message handler.", messageHandler.GetName());
            }
        }
    }

    private async Task StopServiceBusMessageHandlers(CancellationToken cancellationToken)
    {
        foreach (var messageHandler in _messageHandlers)
        {
            try
            {
                await messageHandler.StopAsync(cancellationToken);
                _logger.LogInformation("Stopped Service Bus message handler for {HandlerName}.", messageHandler.GetName());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to stop the {HandlerName} Service Bus message handler.", messageHandler.GetName());
            }
        }
    }
}