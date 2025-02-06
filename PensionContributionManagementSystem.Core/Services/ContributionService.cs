using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionContributionManagementSystem.Core.Abstractions;
using PensionContributionManagementSystem.Core.Dtos;
using PensionContributionManagementSystem.Domain.Constants;
using PensionContributionManagementSystem.Domain.Entities;

namespace PensionContributionManagementSystem.Core.Services
{
    public class ContributionService : IContributionService
    {
        private readonly IRepository<Contribution> _contributionRepository;
        private readonly IRepository<TransactionHistory> _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ContributionService> _logger;

        public ContributionService(
            IRepository<Contribution> contributionRepository,
            IRepository<TransactionHistory> transactionRepository,
            IUnitOfWork unitOfWork,
            ILogger<ContributionService> logger) 
        {
            _contributionRepository = contributionRepository;
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<ContributionResponseDto>> AddContribution(ContributionDto contributionDto)
        {
            _logger.LogInformation("Starting contribution addition for Member ID: {MemberId}", contributionDto.MemberId);

            if (contributionDto.ContributionType == ContributionEnum.Monthly.ToString())
            {
                var existingContribution = _contributionRepository.GetAll()
                    .Where(c => c.MemberId == contributionDto.MemberId && c.ContributionType == ContributionEnum.Monthly.ToString())
                    .Any(c => c.ContributionDate.Month == contributionDto.ContributionDate.Month && c.ContributionDate.Year == contributionDto.ContributionDate.Year);

                if (existingContribution)
                {
                    _logger.LogWarning("Monthly contribution already exists for Member ID: {MemberId} in {Month}/{Year}",
                        contributionDto.MemberId, contributionDto.ContributionDate.Month, contributionDto.ContributionDate.Year);

                    return Result.Failure<ContributionResponseDto>(new[] { new Error("ValidationFailed", "Monthly contribution already exists for this month.") });
                }
            }

            var contribution = new Contribution
            {
                MemberId = contributionDto.MemberId,
                ContributionType = contributionDto.ContributionType,
                Amount = contributionDto.Amount,
                ContributionDate = contributionDto.ContributionDate,
                ReferenceNumber = contributionDto.ReferenceNumber
            };

            await _contributionRepository.Add(contribution);
            _logger.LogInformation("Contribution added for Member ID: {MemberId}, Amount: {Amount}, Type: {Type}",
                contributionDto.MemberId, contributionDto.Amount, contributionDto.ContributionType);

            await _transactionRepository.Add(new TransactionHistory
            {
                EntityId = contribution.MemberId,
                EntityType = nameof(Contribution),
                ChangeType = "Created",
                ChangeDetails = $"New {contribution.ContributionType} contribution added."
            });

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Contribution successfully saved for Member ID: {MemberId}", contributionDto.MemberId);

            var resultDto = new ContributionResponseDto
            {
                Id = contribution.Id,
                MemberId = contribution.MemberId,
                ContributionType = contribution.ContributionType,
                Amount = contribution.Amount,
                ContributionDate = contribution.ContributionDate,
                ReferenceNumber = contribution.ReferenceNumber
            };

            return Result.Success(resultDto);
        }

        public async Task<Result<IEnumerable<ContributionResponseDto>>> GetMemberContributions(string memberId, int pageSize, int offset)
        {
            _logger.LogInformation("Fetching contributions for Member ID: {MemberId}", memberId);

            var contributions = _contributionRepository.GetAll()
                .Where(c => c.MemberId == memberId)
                .Select(c => new ContributionResponseDto
                {
                    Id = c.Id,
                    MemberId = c.MemberId,
                    ContributionType = c.ContributionType,
                    Amount = c.Amount,
                    ContributionDate = c.ContributionDate,
                    ReferenceNumber = c.ReferenceNumber
                });

            var pagedContributions = await contributions
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync();

            if (!pagedContributions.Any())
            {
                _logger.LogWarning("No contributions found for Member ID: {MemberId}", memberId);
            }
            else
            {
                _logger.LogInformation("{Count} contributions found for Member ID: {MemberId}", pagedContributions.Count, memberId);
            }

            return Result.Success(pagedContributions.AsEnumerable());
        }


        public async Task<Result<IEnumerable<TransactionHistoryDto>>> GetTransactionHistoryByMemberId(string memberId, int pageSize, int offset)
        {
            _logger.LogInformation("Fetching transaction history for Member ID: {MemberId}, PageSize: {PageSize}, Offset: {Offset}", memberId, pageSize, offset);

            var transactionsQuery = _transactionRepository.GetAll()
                .Where(t => t.EntityId == memberId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TransactionHistoryDto
                {
                    Id = t.Id,
                    EntityId = t.EntityId,
                    EntityType = t.EntityType,
                    ChangeType = t.ChangeType,
                    ChangeDetails = t.ChangeDetails,
                    TimeStamp = t.CreatedAt.ToString(),
                });

            var pagedTransactions = await transactionsQuery
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync();

            if (!pagedTransactions.Any())
            {
                _logger.LogWarning("No transaction history found for Member ID: {MemberId}", memberId);
                return Result.Failure<IEnumerable<TransactionHistoryDto>>(new[] { new Error("NotFound", "No transaction history found.") });
            }

            _logger.LogInformation("{Count} transactions found for Member ID: {MemberId}", pagedTransactions.Count, memberId);
            return Result.Success(pagedTransactions.AsEnumerable());
        }


        public async Task CalculateMonthlyInterest()
        {
            _logger.LogInformation("Starting monthly interest calculation for all contributions.");

            var contributions = _contributionRepository.GetAll().ToList();

            foreach (var contribution in contributions)
            {
                decimal interest = contribution.Amount * 0.05m; // 5% interest rate

                _logger.LogInformation("Calculated interest for Member ID: {MemberId}, Contribution ID: {ContributionId}, Interest: {InterestAmount}",
                    contribution.MemberId, contribution.Id, interest);

                await _transactionRepository.Add(new TransactionHistory
                {
                    EntityId = contribution.MemberId,
                    EntityType = nameof(Contribution),
                    ChangeType = "Updated",
                    ChangeDetails = $"Interest calculated: {interest:C}"
                });
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Monthly interest calculation completed.");
        }
    }
}
