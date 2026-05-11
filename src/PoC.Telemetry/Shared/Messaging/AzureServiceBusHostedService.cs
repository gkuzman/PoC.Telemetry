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
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Keep the service alive until it is stopped
                await Task.Delay(1000, stoppingToken);
                await ManageServiceBusMessageHandlers(stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            // Graceful shutdown
        }
        finally
        {
            await StopServiceBusMessageHandlers(stoppingToken);
        }
    }

    private async Task ManageServiceBusMessageHandlers(CancellationToken stoppingToken)
    {
        foreach (var messageHandler in _messageHandlers)
        {
            try
            {
                await messageHandler.ManageMessageHandlerStatus(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to update the '{messageHandler.GetName()}' service bus message handler, please review the connection error! Message: {e.Message}");
            }
        }
    }

    private async Task StopServiceBusMessageHandlers(CancellationToken stoppingToken)
    {
        foreach (var messageHandler in _messageHandlers)
        {
            try
            {
                await messageHandler.StopAsync(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to stop the '{messageHandler.GetName()}' service bus message handler, please review the connection error! Message: {e.Message}");
            }
        }
    }
}