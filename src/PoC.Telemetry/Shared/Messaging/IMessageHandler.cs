namespace Shared.Messaging;

public interface IMessageHandler<T>
{
    Task HandleAsync(MessageHandlerArgs<T> args);
}