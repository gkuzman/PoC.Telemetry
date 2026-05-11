using PAM.Controllers;

namespace PAM.Services;

public interface IWithdrawalService
{
    Task Initiate(InitiateWithdrawalMessageRequest request, string correlationId, CancellationToken cancellationToken);
}