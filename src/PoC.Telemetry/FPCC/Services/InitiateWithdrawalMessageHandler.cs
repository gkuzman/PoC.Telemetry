using FPCC.Requests;
using Shared.Contracts;
using Shared.Messaging;

namespace FPCC.Services;

public class InitiateWithdrawalMessageHandler : IMessageHandler<InitiateWithdrawalMessage>
{
    private readonly IWithdrawalService _withdrawalService;

    public InitiateWithdrawalMessageHandler(IWithdrawalService withdrawalService)
    {
        _withdrawalService = withdrawalService;
    }
    public async Task HandleAsync(MessageHandlerArgs<InitiateWithdrawalMessage> args)
    {
        await _withdrawalService.Process(new InitiateWithdrawalRequest(args.Message));
    }
}