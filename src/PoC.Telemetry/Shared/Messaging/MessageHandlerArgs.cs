namespace Shared.Messaging;

public class MessageHandlerArgs<T>
{
    public required T Message { get; init; }
    public required CancellationToken CancellationToken { get; init; }
}