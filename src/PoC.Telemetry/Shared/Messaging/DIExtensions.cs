using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.Messaging;

public static class DIExtensions
{
    public static IServiceCollection AddServiceBusSenderService(this IServiceCollection services)
    {
        services.AddSingleton<IServiceBusSenderService, ServiceBusSenderService>();
        return services;
    }

    public static void AddServiceBusMessageHandler<TServiceBusMessage, TServiceBusMessageHandler>(
        this IServiceCollection services,
        string queueOrTopicName,
        string? topicSubscriptionName = null,
        int maxDeliveryCount = 3)
        where TServiceBusMessageHandler : class, IMessageHandler<TServiceBusMessage>
    {
        // Register the hosted service only once (IHostedService allows multiple registrations of the same type).
        services.TryAddHostedService<AzureServiceBusHostedService>();
        services.AddScoped<TServiceBusMessageHandler>();

        services.AddSingleton<IAzureServiceBusMessageHandler>(serviceProvider =>
        {
            var options = new AzureServiceBusMessageHandlerOptions
            {
                QueueOrTopicName = queueOrTopicName,
                TopicSubscriptionName = topicSubscriptionName,
                MaxDeliveryCount = maxDeliveryCount
            };

            var logger = serviceProvider.GetRequiredService<ILogger<AzureServiceBusMessageHandler<TServiceBusMessage, TServiceBusMessageHandler>>>();
            var serviceBusClient = serviceProvider.GetRequiredService<ServiceBusClient>();
            return new AzureServiceBusMessageHandler<TServiceBusMessage, TServiceBusMessageHandler>(logger, serviceBusClient, options, serviceProvider);
        });
    }

    public static IServiceCollection TryAddHostedService<THostedService>(
        this IServiceCollection services)
        where THostedService : class, IHostedService
    {
        // Only add if not already registered
        if (!services.Any(sd => sd.ServiceType == typeof(IHostedService) &&
                                sd.ImplementationType == typeof(THostedService)))
        {
            services.AddHostedService<THostedService>();
        }

        return services;
    }
}