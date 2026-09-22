using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using DiaryPortfolio.Application.Common;
using DiaryPortfolio.Application.Features.Onboarding.Portfolio.Create;
using DiaryPortfolio.Application.Features.Onboarding.Portfolio.Get;
using DiaryPortfolio.Application.Request;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiaryPortfolio.Api.Controller;

[Route("api/onboarding")]
[ApiController]
public class OnboardingController(IMediator mediator) : ControllerBase
{
    [HttpPost("submit")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<ResultResponse<OnboardingSubmission>>> SubmitOnboardingDetails(
        [FromForm] string  jsonOnboardingDetails,
        [FromForm] IFormFile? profilePhoto,
        CancellationToken cancellationToken)
    {
        var onboardingRequest = JsonSerializer.Deserialize<OnboardingSubmission>(
            jsonOnboardingDetails,
            new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true
            });

        if (onboardingRequest == null)
        {
            return ResultResponse<OnboardingSubmission>
                .Failure(
                    new Error(
                        HttpStatusCode.BadRequest,
                        "Onboarding details not given."));
        }

        if (profilePhoto != null)
        {
            onboardingRequest.Profile.ProfileFileSteam = new MediaStream
            {
                Stream = profilePhoto.OpenReadStream(),
                FileName = profilePhoto.FileName,
            };
        }
        
        // if (profileResume != null)
        // {
        //     profileUpload.ResumeFileStream = new MediaStream
        //     {
        //         Stream = profileResume.OpenReadStream(),
        //         FileName = profileResume.FileName
        //     };
        // }

        var request = new CreatePortfolioOnboardingRequest(onboardingRequest);
        
        return await mediator.Send(request, cancellationToken);
        
    }

    [HttpGet("status")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<ResultResponse<bool>>> OnboardingStatus(
        CancellationToken cancellationToken)
    {
        var request = new GetPortfolioOnboardingRequest();
        
        return await mediator.Send(request, cancellationToken);
    }
    
}