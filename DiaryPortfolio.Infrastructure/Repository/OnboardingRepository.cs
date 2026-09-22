using System.Data;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using DiaryPortfolio.Application.Common;
using DiaryPortfolio.Application.DTOs;
using DiaryPortfolio.Application.IRepository;
using DiaryPortfolio.Application.IServices;
using DiaryPortfolio.Application.Request;
using DiaryPortfolio.Domain.Entities;
using DiaryPortfolio.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DiaryPortfolio.Infrastructure.Repository;

public class OnboardingRepository(
    ApplicationDbContext context, 
    IUserService userService) : IOnboardingRepository
{

    
    public async Task<ResultResponse<OnboardingSubmission>> CreatePortfolioOnboarding(
        OnboardingSubmission request)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userService.UserId, DbType.Guid);
            parameters.Add("@ProfileJson", JsonSerializer.Serialize(request.Profile, jsonOptions), DbType.String);
            parameters.Add("@EducationJson", JsonSerializer.Serialize(request.Educations, jsonOptions), DbType.String);
            parameters.Add("@SkillJson", JsonSerializer.Serialize(request.Skills, jsonOptions), DbType.String);
            parameters.Add("@ExperienceJson", JsonSerializer.Serialize(request.Experiences, jsonOptions), DbType.String);
            
            var connection = context.Database.GetDbConnection(); 
            
            var submission = await connection.QueryAsync(
                "sp_SubmitOnboarding",
                parameters,
                commandType: CommandType.StoredProcedure);

            return ResultResponse<OnboardingSubmission>.Success(request);

        }
        catch (SqlException ex)
        {
            return ResultResponse<OnboardingSubmission>.Failure(
                new Error(
                    HttpStatusCode.InternalServerError,
                    "A database error occurred while processing your request.",
                    request));

        }
        catch (Exception ex)
        {
            return ResultResponse<OnboardingSubmission>.Failure(
                new Error(
                    HttpStatusCode.BadRequest,
                    ex.Message,
                    request));
        }
    }

    public async Task<ResultResponse<bool>> GetPortfolioOnboarding()
    {
        var profileId = userService.PortfolioProfileId;
        
        var hasEducation = await context.Educations
            .AnyAsync(e => e.PortfolioProfileId == profileId);
        
        var hasSkills    = await context.Skills
            .AnyAsync(s => s.PortfolioProfileId == profileId);
        
        var hasExperience = await context.Experiences
            .AnyAsync(ex => ex.PortfolioProfileId == profileId);
        
        var isOnboardingComplete = hasEducation && hasSkills && hasExperience;

        return ResultResponse<bool>.Success(isOnboardingComplete);
    }

}