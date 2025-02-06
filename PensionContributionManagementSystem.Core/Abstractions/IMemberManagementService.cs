using PensionContributionManagementSystem.Core.Dtos;

namespace PensionContributionManagementSystem.Core.Abstractions;

public interface IMemberManagementService
{Task<Result<RegisterResponseDto>> Register(RegisterUserDto registerUserDto);
    Task<Result<LoginResponseDto>> Login(LoginUserDto loginUserDto);
    Task<Result<MemberDto>> GetMemberById(string memberId);
    Task<Result<MemberDto>> UpdateMember(string memberId, UpdateMemberDto updateMemberDto);
    Task<Result> DeleteMember(string memberId);
}