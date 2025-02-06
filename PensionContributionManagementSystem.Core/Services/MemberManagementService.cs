using PensionContributionManagementSystem.Core.Abstractions;
using PensionContributionManagementSystem.Core.Dtos;
using PensionContributionManagementSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace PensionContributionManagementSystem.Core.Services
{
    public class MemberManagementService : IMemberManagementService
    {
        private readonly IJwtService _jwtService;
        private readonly UserManager<Member> _userManager;
        private readonly ILogger<MemberManagementService> _logger;

        public MemberManagementService(UserManager<Member> userManager, IJwtService jwtService, ILogger<MemberManagementService> logger)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<Result<RegisterResponseDto>> Register(RegisterUserDto registerUserDto)
        {
            _logger.LogInformation("Starting registration for user with email: {Email}", registerUserDto.Email);

            var user = Member.Create(registerUserDto.FirstName, registerUserDto.LastName, registerUserDto.Email, registerUserDto.DateOfBirth, registerUserDto.EmployerId);

            var result = await _userManager.CreateAsync(user, registerUserDto.Password);
            if (!result.Succeeded)
            {
                _logger.LogWarning("User registration failed for email: {Email}. Errors: {Errors}", registerUserDto.Email, result.Errors.Select(e => e.Description));
                return Result.Failure<RegisterResponseDto>(result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray());
            }

            var newUser = await _userManager.FindByEmailAsync(registerUserDto.Email);
            if (newUser == null)
            {
                _logger.LogError("Failed to find newly registered user with email: {Email}", registerUserDto.Email);
                return Result.Failure<RegisterResponseDto>(new[] { new Error("UserError", "Failed to find user after creation.") });
            }

            _logger.LogInformation("User registered successfully with ID: {UserId}", newUser.Id);

            var resultDto = new RegisterResponseDto
            {
                Id = newUser.Id,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Email = newUser.Email!,
                DateOfBirth = newUser.DateOfBirth,
                CreatedAt = newUser.CreatedAt
            };

            return Result.Success(resultDto);
        }

        public async Task<Result<LoginResponseDto>> Login(LoginUserDto loginUserDto)
        {
            _logger.LogInformation("Attempting login for user with email: {Email}", loginUserDto.Email);

            var user = await _userManager.FindByEmailAsync(loginUserDto.Email);
            if (user is null)
            {
                _logger.LogWarning("Login failed for email: {Email}. User not found.", loginUserDto.Email);
                return new Error[] { new("MemberManagement.Error", "Email or password not correct") };
            }

            var isValidUser = await _userManager.CheckPasswordAsync(user, loginUserDto.Password);
            if (!isValidUser)
            {
                _logger.LogWarning("Login failed for email: {Email}. Incorrect password.", loginUserDto.Email);
                return new Error[] { new("MemberManagement.Error", "Email or password not correct") };
            }

            var token = _jwtService.GenerateToken(user);
            _logger.LogInformation("User logged in successfully with email: {Email}", loginUserDto.Email);

            return new LoginResponseDto(token);
        }

        public async Task<Result<MemberDto>> GetMemberById(string memberId)
        {
            _logger.LogInformation("Fetching member details for ID: {MemberId}", memberId);

            var user = await _userManager.FindByIdAsync(memberId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("Member not found or deleted. ID: {MemberId}", memberId);
                return Result.Failure<MemberDto>(new[] { new Error("MemberManagement.NotFound", "Member not found or deleted") });
            }

            _logger.LogInformation("Member found with ID: {MemberId}", memberId);

            return Result.Success(new MemberDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                CreatedAt = user.CreatedAt
            });
        }

        public async Task<Result<MemberDto>> UpdateMember(string memberId, UpdateMemberDto updateMemberDto)
        {
            _logger.LogInformation("Updating member details for ID: {MemberId}", memberId);

            var user = await _userManager.FindByIdAsync(memberId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("Update failed. Member not found or deleted. ID: {MemberId}", memberId);
                return Result.Failure<MemberDto>(new[] { new Error("MemberManagement.NotFound", "Member not found or deleted") });
            }

            user.FirstName = updateMemberDto.FirstName ?? user.FirstName;
            user.LastName = updateMemberDto.LastName ?? user.LastName;
            user.Email = updateMemberDto.Email ?? user.Email;
            user.UserName = updateMemberDto.Email ?? user.Email;
            user.DateOfBirth = updateMemberDto.DateOfBirth ?? user.DateOfBirth;
            user.EmployerId = updateMemberDto.EmployerId ?? user.EmployerId;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update member with ID: {MemberId}. Errors: {Errors}", memberId, result.Errors.Select(e => e.Description));
                return Result.Failure<MemberDto>(result.Errors.Select(e => new Error(e.Code, e.Description)).ToArray());
            }

            _logger.LogInformation("Member updated successfully. ID: {MemberId}", memberId);

            return Result.Success(new MemberDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                CreatedAt = user.CreatedAt
            });
        }

        public async Task<Result> DeleteMember(string memberId)
        {
            _logger.LogInformation("Deleting member with ID: {MemberId}", memberId);

            var user = await _userManager.FindByIdAsync(memberId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("Delete operation failed. Member not found or already deleted. ID: {MemberId}", memberId);
                return Result.Failure(new[] { new Error("MemberManagement.NotFound", "Member not found or already deleted") });
            }

            user.IsDeleted = true;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to delete member with ID: {MemberId}. Errors: {Errors}", memberId, result.Errors.Select(e => e.Description));
                return Result.Failure(result.Errors.Select(e => new Error(e.Code, e.Description)).ToArray());
            }

            _logger.LogInformation("Member successfully marked as deleted. ID: {MemberId}", memberId);
            return Result.Success();
        }
    }
}
