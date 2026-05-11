using Microsoft.EntityFrameworkCore;
using PAM.Controllers;
using PAM.DB;
using PAM.DB.Models;
using Shared;
using Shared.Contracts;
using Shared.Messaging;

namespace PAM.Services;

public class WithdrawalService : IWithdrawalService
{
    private readonly PamDbContext _dbContext;
    private readonly IServiceBusSenderService _senderService;
    private readonly ILogger<WithdrawalService> _logger;

    public WithdrawalService(PamDbContext dbContext,  IServiceBusSenderService senderService, ILogger<WithdrawalService> logger)
    {
        _dbContext = dbContext;
        _senderService = senderService;
        _logger = logger;
    }

    public async Task Initiate(InitiateWithdrawalMessageRequest request, string correlationId, CancellationToken cancellationToken)
    {
        var account = await _dbContext.Accounts
            .Include(a => a.Wallet)
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account is null)
        {
            _logger.LogInformation(
                "Account {AccountId} not found, creating new account. CorrelationId: {CorrelationId}",
                request.AccountId, correlationId);

            account = new Account
            {
                Id = request.AccountId,
                Wallet = new Wallet
                {
                    AccountId = request.AccountId,
                    Balance = 1000m,
                    Reserved = 0m
                }
            };
            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        account.Wallet!.Reserve(request.Amount);
        await _dbContext.SaveChangesAsync(cancellationToken);

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
    }
}