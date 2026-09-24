using DiaryPortfolio.Application.IRepository;
using DiaryPortfolio.Application.IServices;
using DiaryPortfolio.Domain.Entities;
using DiaryPortfolio.Domain.Enum;
using DiaryPortfolio.Domain.Interfaces;
using DiaryPortfolio.Infrastructure.Data;
using DiaryPortfolio.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Reflection;

namespace DiaryPortfolio.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {

        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly string _connectionString;
        //private readonly UserManager<UserModel> _userManager;

        public UserRepository(
            ApplicationDbContext context,
            IUserService userService
            //,
            //UserManager<UserModel> userManager
        )
        {
            _context = context;
            _userService = userService;
            _connectionString = context.Database.GetConnectionString() ?? "";
            //_userManager = userManager;
        }

        public async Task EnsureOwnerAsync(
            Guid resourceId, 
            Type resourceType)
        {
            object? entity;

            // Check if this type knows how to load its own ownership chain
            var withIncludesMethod = resourceType.GetMethod(
                nameof(IUserOwnerQuery.WithOwnerIncludes),
                BindingFlags.Public | BindingFlags.Static);

            if (withIncludesMethod != null)
            {
                // Use the type's own include query, then filter by PK
                var query = (IQueryable<object>)withIncludesMethod.Invoke(null, [_context])!;
                entity = await query
                    .Where(e => EF.Property<Guid>(e, "Id") == resourceId)
                    .FirstOrDefaultAsync();
            }
            else
            {
                // Fallback: simple FindAsync for flat entities
                entity = await _context.FindAsync(resourceType, resourceId);
            }

            if (entity == null)
            {
                return;
                //throw new AppException(
                //    "Resource not found.",
                //    HttpStatusCode.NotFound);
            }

            if (entity is not IUserOwner userOwner)
            {
                throw new InvalidOperationException(
                    $"{resourceType.Name} does not implement IUserOwner " +
                    $"but is using the implementation.");
            }

            if (userOwner.OwnerId != (_userService?.UserId ?? Guid.Empty))
            {
                throw new AppException(
                    "User does not own this resource.",
                    HttpStatusCode.Forbidden);
            }
                
        }

        public async Task<UserModel?> GetUserByUserId(
            Guid userId, 
            ProfileType profileType)
        {
            var query = _context.Users.AsQueryable();

            if (profileType == ProfileType.Diary)
            {
                query = query
                    .Include(u => u.DiaryProfile);
            }

            if (profileType == ProfileType.Portfolio)
            {
                query = query
                    .Include(u => u.PortfolioProfile)
                        .ThenInclude(p => p.ProfilePhoto)
                    .Include(u => u.PortfolioProfile)
                        .ThenInclude(p => p.Resume)
                        .ThenInclude(f => f.ResumeFile);
            }

            if (profileType == ProfileType.All)
            {
                query = query
                    .Include(u => u.PortfolioProfile)
                        .ThenInclude(p => p.ProfilePhoto)
                    .Include(u => u.PortfolioProfile)
                        .ThenInclude(p => p.Resume)
                        .ThenInclude(f => f.ResumeFile)
                    .Include(u => u.DiaryProfile);
            }


            var user = await query
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user;
        }

        // public async Task<UserModel?> GetUserByUserId(
        //     Guid userId,
        //     ProfileType profileType)
        // {
        //     using (var connection = new SqlConnection(_connectionString))
        //     {
        //         var parameters = new
        //         {
        //             UserId = userId,
        //             ProfileType = profileType.ToString(),
        //         };
        //
        //         using (var multi = await connection.QueryMultipleAsync(
        //             StoredProcedures.Profile.GetPortfolioProfile,
        //             parameters,
        //             commandType: System.Data.CommandType.StoredProcedure))
        //         {
        //             var user = await multi.ReadFirstOrDefaultAsync<UserModel>();
        //
        //             if (user == null) { return null; }
        //
        //             // 2. Read Portfolio Data if applicable
        //             if (profileType == ProfileType.Portfolio 
        //                 || profileType == ProfileType.All)
        //             {
        //                 user.PortfolioProfile = await multi
        //                     .ReadFirstOrDefaultAsync<PortfolioProfileModel>();
        //
        //                 // Read Location (Third Result Set)
        //                 if (user.PortfolioProfile != null)
        //                 {
        //                     user.PortfolioProfile.Location = await multi
        //                         .ReadFirstOrDefaultAsync<LocationModel>();
        //
        //                     // Read Photo (Fourth Result Set)
        //                     user.PortfolioProfile.ProfilePhoto = await multi
        //                         .ReadFirstOrDefaultAsync<PhotoModel>();
        //                 }
        //             }
        //
        //             // 3. Read Diary Data if applicable (Matches the last IF in your SQL)
        //             if (profileType == ProfileType.Diary 
        //                 || profileType == ProfileType.All)
        //             {
        //                 user.DiaryProfile = await multi
        //                     .ReadFirstOrDefaultAsync<DiaryProfileModel>();
        //             }
        //
        //             return user;
        //         }
        //
        //     }
        // }

        public async Task<UserModel?> GetUserByUsername(
            string username, ProfileType profileType)
        {
            var query = _context.Users.AsQueryable();

            if (profileType == ProfileType.Diary)
            {
                query = query
                    .Include(u => u.DiaryProfile);
            }
            
            if (profileType == ProfileType.Portfolio)
            {
                query = query
                    .Include(u => u.PortfolioProfile)
                        .ThenInclude(p => p.ProfilePhoto)
                    .Include(u => u.PortfolioProfile)
                        .ThenInclude(p => p.Resume)
                        .ThenInclude(f => f.ResumeFile);
            }

            if (profileType == ProfileType.All)
            {
                query = query
                    .Include(u => u.PortfolioProfile)
                    .Include(u => u.DiaryProfile);
            }

            var user = await query
                .FirstOrDefaultAsync(u => u.UserName == username);

            return user;
        }
    }
}