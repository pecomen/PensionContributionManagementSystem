using PensionContributionManagementSystem.Core.Dtos;
using PensionContributionManagementSystem.Domain.Entities;

namespace PensionContributionManagementSystem.Core.Abstractions
{
    public interface IContributionService
    {
        Task<Result<ContributionResponseDto>> AddContribution(ContributionDto contribution);
        Task<Result<IEnumerable<ContributionResponseDto>>> GetMemberContributions(string memberId, int pageSize = 10, int offset = 0);

        Task<Result<IEnumerable<TransactionHistoryDto>>> GetTransactionHistoryByMemberId(string memberId, int pageSize = 10, int offset =0);
        Task CalculateMonthlyInterest();
    }

}

