using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shared.Messaging;

internal class AzureServiceBusMessageHandler<TServiceBusMessageType, TServiceBusMessageHandler> : IAzureServiceBusMessageHandler, IAsyncDisposable
    where TServiceBusMessageHandler : IMessageHandler<TServiceBusMessageType>
{
  private readonly ServiceBusProcessor _processor;
  private readonly ILogger<AzureServiceBusMessageHandler<TServiceBusMessageType, TServiceBusMessageHandler>> _logger;
  private readonly AzureServiceBusMessageHandlerOptions _options;
  private readonly IServiceProvider _serviceProvider;
  private bool _isProcessing = false;

  public AzureServiceBusMessageHandler(
    ILogger<AzureServiceBusMessageHandler<TServiceBusMessageType, TServiceBusMessageHandler>> logger,
    ServiceBusClient serviceBusClient,
    AzureServiceBusMessageHandlerOptions options,
    IServiceProvider serviceProvider)
  {
    _logger = logger;
    _options = options;
    _serviceProvider = serviceProvider;
    var processorOptions = new ServiceBusProcessorOptions
    {
      MaxAutoLockRenewalDuration = Timeout.InfiniteTimeSpan
    };

    _processor = string.IsNullOrEmpty(_options.TopicSubscriptionName) ?
      serviceBusClient.CreateProcessor(_options.QueueOrTopicName, processorOptions) :
      serviceBusClient.CreateProcessor(_options.QueueOrTopicName, _options.TopicSubscriptionName, processorOptions);

    _processor.ProcessMessageAsync += ProcessMessage;
    _processor.ProcessErrorAsync += ErrorHandler;
  }


  public async Task ManageMessageHandlerStatus(CancellationToken cancellationToken)
  {
    switch (_isProcessing)
    {
      case false:
        await StartAsync(cancellationToken);
        break;
      case true:
        await StopAsync(cancellationToken);
        break;
    }
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    await _processor.StartProcessingAsync(cancellationToken);
    _isProcessing = true;
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    await _processor!.StopProcessingAsync(cancellationToken);
    _isProcessing = false;
  }

  public string GetName()
  {
    return _options.QueueOrTopicName;
  }

  private Task ErrorHandler(ProcessErrorEventArgs arg)
  {
    _logger.LogError(arg.Exception, "Exception in ServiceBusProcessor. ErrorSource={ErrorSource} EntityPath={EntityPath}", arg.ErrorSource, arg.EntityPath);
    return Task.CompletedTask;
  }

  private async Task ProcessMessage(ProcessMessageEventArgs args)
  {
    using var loggerScope = _logger.BeginScope(new Dictionary<string, object>
    {
      ["MessageId"] = args.Message.MessageId,
      ["CorrelationId"] = args.Message.CorrelationId,
      ["DeliveryCount"] = args.Message.DeliveryCount,
      ["EnqueuedTime"] = args.Message.EnqueuedTime
    });

    _logger.LogInformation("Processing message from Service Bus.");

    try
    {
      var messageString = args.Message.Body.ToString();
      var message = JsonSerializer.Deserialize<TServiceBusMessageType>(messageString);
      if (message is null)
      {
        _logger.LogError("Unable to deserialize message of type {Type}", typeof(TServiceBusMessageType).FullName);
        throw new ArgumentException($"Unable to deserialize message of type {typeof(TServiceBusMessageType).FullName}", nameof(args));
      }

      await using var scope = _serviceProvider.CreateAsyncScope();
      var messageHandler = scope.ServiceProvider.GetRequiredService<TServiceBusMessageHandler>();
      await messageHandler.HandleAsync(new MessageHandlerArgs<TServiceBusMessageType> { Message = message, CancellationToken = args.CancellationToken });
    }
    catch (Exception ex)
    {
      await HandleException(args, ex, _options.QueueOrTopicName);
    }
    finally
    {
    }
  }

  private async Task HandleException(ProcessMessageEventArgs args, Exception ex, string handler)
  {

    if (_options.MaxDeliveryCount >= args.Message.DeliveryCount)
    {
      _logger.LogError(ex, "Exception while processing service bus message. Abandoning message for later retry. Handler={handler}", handler);
      await args.AbandonMessageAsync(args.Message);
    }
    else
    {
      _logger.LogError(ex, "Exception while processing service bus message. Delivery count exceeded putting message on dead letter queue. Handler={handler}", handler);
      await args.DeadLetterMessageAsync(args.Message, $"Exception while processing service bus message. Handler={handler}", TruncateString(ex.ToString(), 4096));
    }
  }

  private static string TruncateString(string value, int maxLength)
  {
    if (value.Length > maxLength)
    {
      return value.Substring(0, maxLength);
    }

    return value;
  }

  public async ValueTask DisposeAsync()
  {
    if (_processor is not null)
    {
      await _processor.DisposeAsync();
    }

    GC.SuppressFinalize(this);
  }
}