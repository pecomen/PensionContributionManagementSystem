using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionContributionManagementSystem.Core.Abstractions;
using PensionContributionManagementSystem.Core.Dtos;
using PensionContributionManagementSystem.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PensionContributionManagementSystem.Core.Services
{
    public class EmployerService : IEmployerService
    {
        private readonly IRepository<Employer> _employerRepository;
        private readonly UserManager<Member> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmployerService> _logger;

        public EmployerService(
            IRepository<Employer> employerRepository,
            UserManager<Member> userManager,
            IUnitOfWork unitOfWork,
            ILogger<EmployerService> logger) 
        {
            _employerRepository = employerRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<EmployerResponseDto>> AddEmployer(AddEmployerDto employerDto)
        {
            _logger.LogInformation("Starting to add a new employer: {CompanyName}", employerDto.CompanyName);
            var employerExist = await _employerRepository.GetAll().Where(c => c.RegistrationNumber == employerDto.RegistrationNumber ).FirstOrDefaultAsync();
            if (employerExist != null)
            {
                _logger.LogWarning("Employer with  {RegistrationNumber} already exist.", employerDto.RegistrationNumber);
                return Result.Failure<EmployerResponseDto>(new[] { new Error("Duplicate", $"Employer with  {employerDto.RegistrationNumber} already exist.") });
            }

            var employer = new Employer
            {
                CompanyName = employerDto.CompanyName,
                RegistrationNumber = employerDto.RegistrationNumber,
                IsActive = true
            };

            await _employerRepository.Add(employer);
            _logger.LogInformation("Employer {CompanyName} added with Registration Number: {RegistrationNumber}",
                employer.CompanyName, employer.RegistrationNumber);

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Employer {CompanyName} successfully saved to the database.", employer.CompanyName);

            var responseDto = new EmployerResponseDto
            {
                Id = employer.Id,
                CompanyName = employer.CompanyName,
                RegistrationNumber = employer.RegistrationNumber,
                IsActive = employer.IsActive
            };

            return Result.Success(responseDto);
        }

        public async Task<Result<EmployerDto>> GetEmployerWithMembers(string employerId)
        {
            _logger.LogInformation("Fetching employer details for Employer ID: {EmployerId}", employerId);

            var employer = await _employerRepository.FindById(employerId);
            if (employer == null)
            {
                _logger.LogWarning("Employer with ID {EmployerId} not found.", employerId);
                return Result.Failure<EmployerDto>(new[] { new Error("NotFound", "Employer not found.") });
            }

            var members = await _userManager.Users
                .Where(m => m.EmployerId == employerId)
                .ToListAsync();

            _logger.LogInformation("{MemberCount} members found for Employer ID: {EmployerId}", members.Count, employerId);

            var employerDto = new EmployerDto
            {
                Id = employer.Id,
                CompanyName = employer.CompanyName,
                RegistrationNumber = employer.RegistrationNumber,
                IsActive = employer.IsActive,
                Members = members.Select(m => new MemberDto
                {
                    Id = m.Id,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    Email = m.Email,
                    DateOfBirth = m.DateOfBirth,
                    CreatedAt = m.CreatedAt
                }).ToList()
            };

            return Result.Success(employerDto);
        }
    }
}
