using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace Shared.Messaging;

public class ServiceBusSenderService : IServiceBusSenderService
{
    private readonly ServiceBusClient _client;

    public ServiceBusSenderService(ServiceBusClient client)
    {
        _client = client;
    }

    public async Task Send<T>(T payload, string queueName, CancellationToken cancellationToken = default)
    {
        var serialized = JsonSerializer.Serialize(payload);
        var message = new ServiceBusMessage(serialized);
        await using var sender = _client.CreateSender(queueName);
        await sender.SendMessageAsync(message, cancellationToken);
    }
}