using FPCC.Requests;

namespace FPCC.Services;

public interface IWithdrawalService
{
    Task Process(InitiateWithdrawalRequest request);
}