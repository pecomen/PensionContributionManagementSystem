using Microsoft.Extensions.Logging;
using PensionContributionManagementSystem.Core.Abstractions;
using PensionContributionManagementSystem.Core.Dtos;
using PensionContributionManagementSystem.Domain.Entities;

public class BenefitService : IBenefitService
{
    private readonly IRepository<Benefit> _benefitRepository;
    private readonly IRepository<Contribution> _contributionRepository;
    private readonly IRepository<TransactionHistory> _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BenefitService> _logger;

    public BenefitService(
        IRepository<Benefit> benefitRepository,
        IRepository<Contribution> contributionRepository,
        IRepository<TransactionHistory> transactionRepository,
        IUnitOfWork unitOfWork,
        ILogger<BenefitService> logger) 
    {
        _benefitRepository = benefitRepository;
        _contributionRepository = contributionRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BenefitDto>> CalculateBenefit(string memberId)
    {
        _logger.LogInformation("Starting benefit calculation for member: {MemberId}", memberId);

        var contributions = _contributionRepository.GetAll().Where(c => c.MemberId == memberId).ToList();

        if (!contributions.Any())
        {
            _logger.LogWarning("No contributions found for member: {MemberId}", memberId);
            return Result.Failure<BenefitDto>(new[] { new Error("CalculationFailed", "No contributions found for the member.") });
        }

        decimal totalContributions = contributions.Sum(c => c.Amount);
        _logger.LogInformation("Total contributions for member {MemberId}: {TotalContributions}", memberId, totalContributions);

        var eligibility = totalContributions >= 100000 ? "Eligible" : "Not Eligible";
        var benefitAmount = eligibility == "Eligible" ? totalContributions * 0.1m : 0;

        var benefit = new Benefit
        {
            MemberId = memberId,
            BenefitType = "Retirement",
            CalculationDate = DateTime.UtcNow,
            EligibilityStatus = eligibility,
            Amount = benefitAmount
        };

        await _benefitRepository.Add(benefit);
        _logger.LogInformation("Benefit record created for member {MemberId}: {BenefitAmount} ({Eligibility})",
            memberId, benefitAmount, eligibility);

        await _transactionRepository.Add(new TransactionHistory
        {
            EntityId = memberId,
            EntityType = nameof(Benefit),
            ChangeType = "Created",
            ChangeDetails = "Benefit calculated."
        });

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Benefit calculation saved for member: {MemberId}", memberId);

        var result = new BenefitDto
        {
            Id = benefit.Id,
            MemberId = benefit.MemberId,
            BenefitType = benefit.BenefitType,
            Amount = benefit.Amount,
            EligibilityStatus = benefit.EligibilityStatus,
            CalculationDate = benefit.CalculationDate,
        };

        return Result.Success(result);
    }

    public async Task UpdateEligibilityStatus()
    {
        _logger.LogInformation("Updating eligibility status for all benefits.");

        var benefits = _benefitRepository.GetAll().ToList();

        foreach (var benefit in benefits)
        {
            var oldStatus = benefit.EligibilityStatus;
            benefit.EligibilityStatus = benefit.Amount >= 100000 ? "Eligible" : "Not Eligible";

            _logger.LogInformation("Updated eligibility for Member {MemberId}: {OldStatus} -> {NewStatus}",
                benefit.MemberId, oldStatus, benefit.EligibilityStatus);

            await _transactionRepository.Add(new TransactionHistory
            {
                EntityId = benefit.MemberId,
                EntityType = nameof(Benefit),
                ChangeType = "Updated",
                ChangeDetails = "Benefit eligibility updated."
            });
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Eligibility status update completed.");
    }
}
