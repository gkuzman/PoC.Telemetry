namespace Shared.Messaging;

public interface IServiceBusSenderService
{
  Task Send<T>(T payload, string queueName, CancellationToken cancellationToken = default);
}