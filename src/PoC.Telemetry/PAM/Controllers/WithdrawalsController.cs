using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PAM.Services;
using Shared.Models;

namespace PAM.Controllers;

public sealed record InitiateWithdrawalMessageRequest(int AccountId, int WithdrawalId, decimal Amount);

[ApiController]
[Route("api/[controller]/[action]")]
public sealed class WithdrawalsController : ControllerBase
{
    private readonly ILogger<WithdrawalsController> _logger;
    private readonly IWithdrawalService _withdrawalService;

    public WithdrawalsController(
        ILogger<WithdrawalsController> logger, IWithdrawalService withdrawalService)
    {
        _logger = logger;
        _withdrawalService = withdrawalService;
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
        await _withdrawalService.Initiate(request, correlationId, cancellationToken);
        return Accepted();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Confirm(
        [FromBody] ConfirmWithdrawal request,
        CancellationToken cancellationToken)
    {
        if (request.AccountId > 3)
        {
            using var activity = TracingExtensions.Source.StartActivity("Release wallet funds", ActivityKind.Server);
        }

        if (request.AccountId <= 0)
            return BadRequest("AccountId must be a positive integer.");

        if (request.WithdrawalId <= 0)
            return BadRequest("WithdrawalId must be a positive integer.");
        return Accepted();
    }
}

