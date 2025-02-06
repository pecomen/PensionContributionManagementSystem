using FluentValidation;
using PensionContributionManagementSystem.Core.Dtos;

public class PaginationDtoValidator : AbstractValidator<PaginationDto>
{
    public PaginationDtoValidator()
    {
        RuleFor(x => x.pageSize)
            .GreaterThan(0).WithMessage("pageSize must be greater than 0.")
            .When(x => x.pageSize <= 0)
            .OverridePropertyName("pageSize"); 
        RuleFor(x => x.offset)
            .GreaterThanOrEqualTo(0).WithMessage("offset cannot be negative.");
    }
}
