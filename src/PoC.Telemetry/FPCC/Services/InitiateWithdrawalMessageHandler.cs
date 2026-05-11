using Shared.Contracts;
using Shared.Messaging;

namespace FPCC.Services;

public class InitiateWithdrawalMessageHandler : IMessageHandler<InitiateWithdrawalMessage>
{
    public async Task HandleAsync(MessageHandlerArgs<InitiateWithdrawalMessage> args)
    {
        await Task.CompletedTask;
    }
}