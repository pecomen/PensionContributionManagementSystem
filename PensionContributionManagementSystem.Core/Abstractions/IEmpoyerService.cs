using PensionContributionManagementSystem.Core.Dtos;
using PensionContributionManagementSystem.Domain.Entities;

namespace PensionContributionManagementSystem.Core.Abstractions
{
    public interface IEmployerService
    {
        Task<Result<EmployerResponseDto>> AddEmployer(AddEmployerDto employer);
        Task<Result<EmployerDto>> GetEmployerWithMembers(string employerId);
    }
}
