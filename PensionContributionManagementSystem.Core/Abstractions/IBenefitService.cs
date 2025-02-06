using PensionContributionManagementSystem.Core.Dtos;
using PensionContributionManagementSystem.Domain.Entities;
namespace PensionContributionManagementSystem.Core.Abstractions
{
    public interface IBenefitService
    {
        Task<Result<BenefitDto>> CalculateBenefit(string memberId);

        Task UpdateEligibilityStatus();
    }
}
