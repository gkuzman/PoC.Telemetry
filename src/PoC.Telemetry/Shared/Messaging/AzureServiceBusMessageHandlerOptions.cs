namespace Shared.Messaging;

public class AzureServiceBusMessageHandlerOptions
{
    public required string QueueOrTopicName { get; set; }
    public string? TopicSubscriptionName { get; set; }
    public int MaxDeliveryCount { get; set; } = 5;
}