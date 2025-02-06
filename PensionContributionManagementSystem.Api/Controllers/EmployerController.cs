using PensionContributionManagementSystem.Core.Abstractions;
using PensionContributionManagementSystem.Core.Dtos;
using PensionContributionManagementSystem.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace PensionContributionManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/employers")]
    public class EmployerController : ControllerBase
    {
        private readonly IEmployerService _employerService;

        public EmployerController(IEmployerService employerService)
        {
            _employerService = employerService;
        }

        /// <summary>
        /// Adds a new employer.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddEmployer([FromBody] AddEmployerDto employerDto)
        {
            var result = await _employerService.AddEmployer(employerDto);
            if (!result.IsSuccess)
                return BadRequest(ResponseDto<EmployerResponseDto>.Failure(result.Errors));

            return CreatedAtAction(
                nameof(GetEmployerWithMembers),
                new { employerId = result.Data.Id },
                ResponseDto<EmployerResponseDto>.Success(result.Data, "Employer created successfully")
            );
        }

        /// <summary>
        /// Retrieves an employer and its members.
        /// </summary>
        [HttpGet("{employerId}")]
        public async Task<IActionResult> GetEmployerWithMembers(string employerId)
        {
            var result = await _employerService.GetEmployerWithMembers(employerId);
            if (!result.IsSuccess)
                return NotFound(ResponseDto<EmployerDto>.Failure(result.Errors, (int)HttpStatusCode.NotFound));

            return Ok(ResponseDto<EmployerDto>.Success(result.Data, "Employer retrieved successfully"));
        }
    }
}
