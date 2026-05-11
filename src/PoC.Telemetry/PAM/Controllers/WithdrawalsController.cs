using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Contracts;
using Shared.Messaging;

namespace PAM.Controllers;

public sealed record InitiateWithdrawalMessageRequest(int AccountId, int WithdrawalId, decimal Amount);

[ApiController]
[Route("api/[controller]/[action]")]
public sealed class WithdrawalsController : ControllerBase
{
    private readonly ILogger<WithdrawalsController> _logger;
    private readonly IServiceBusSenderService _senderService;

    public WithdrawalsController(
        ILogger<WithdrawalsController> logger, IServiceBusSenderService senderService)
    {
        _logger = logger;
        _senderService = senderService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Initiate(
        [FromBody] InitiateWithdrawalMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (request.AccountId <= 0)
            return BadRequest("AccountId must be a positive integer.");

        if (request.WithdrawalId <= 0)
            return BadRequest("WithdrawalId must be a positive integer.");

        if (request.Amount <= 0)
            return BadRequest("Amount must be a positive number.");

        var correlationId = HttpContext.TraceIdentifier;

        var @event = new InitiateWithdrawalMessage(
            AccountId: request.AccountId,
            WithdrawalId: request.WithdrawalId,
            Amount: request.Amount,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: correlationId);

        _logger.LogInformation(
            "Publishing {EventType} for AccountId {AccountId} with CorrelationId {CorrelationId}",
            nameof(InitiateWithdrawalMessage), request.AccountId, correlationId);

        await _senderService.Send(@event, Const.WithdrawalIncomingQueueName, cancellationToken);

        return Accepted();
    }
}

