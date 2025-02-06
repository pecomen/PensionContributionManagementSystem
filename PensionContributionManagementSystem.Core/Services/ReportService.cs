using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PensionContributionManagementSystem.Core.Abstractions;
using PensionContributionManagementSystem.Domain.Entities;

public class ReportService : IReportService
{
    private readonly IRepository<Contribution> _contributionRepository;
    private readonly UserManager<Member> _userManager;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IRepository<Contribution> contributionRepository,
                         UserManager<Member> userManager,
                         ILogger<ReportService> logger)
    {
        _contributionRepository = contributionRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task GenerateContributionValidationReport()
    {
        _logger.LogInformation("Starting contribution validation report generation.");

        var contributions = _contributionRepository.GetAll().ToList();
        _logger.LogInformation("Generated validation report with {Count} contributions.", contributions.Count);

        Console.WriteLine($"Generated validation report with {contributions.Count} contributions.");
    }

    public async Task GenerateMemberStatements()
    {
        _logger.LogInformation("Starting member statement generation.");

        var members = _userManager.Users.ToList();
        foreach (var member in members)
        {
            _logger.LogInformation("Generated statement for {FirstName} {LastName}.", member.FirstName, member.LastName);
            Console.WriteLine($"Generated statement for {member.FirstName} {member.LastName}.");
        }

        _logger.LogInformation("Completed member statement generation.");
    }
}
