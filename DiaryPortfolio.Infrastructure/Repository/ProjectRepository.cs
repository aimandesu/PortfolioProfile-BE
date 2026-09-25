using DiaryPortfolio.Application.Common;
using DiaryPortfolio.Application.IRepository;
using DiaryPortfolio.Application.IServices;
using DiaryPortfolio.Application.Request;
using DiaryPortfolio.Domain.Entities;
using DiaryPortfolio.Domain.Enum;
using DiaryPortfolio.Infrastructure.Data;
using DiaryPortfolio.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiaryPortfolio.Infrastructure.Repository
{
    internal class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ISelectionHelper _selectionHelper;
        private readonly IUserService _userService;

        public ProjectRepository(
            ApplicationDbContext context,
            ISelectionHelper selectionHelper,
            IUserService userService)
        {
            _context = context;
            _selectionHelper = selectionHelper;
            _userService = userService;
        }

        public async Task<ResultResponse<ProjectModel>> CreateProject(
            ProjectUpload projectUpload, 
            FileModel? file, 
            List<VideoModel> videos, 
            List<PhotoModel> photos)
        {
            try
            {
                var selectionResult = await _selectionHelper.GetSelectionResultAsync(
                FilesEnum.Project);

                var projectFile = new FileModel
                {
                    Url = file?.Url ?? "",
                    SelectionId = selectionResult?.Id ?? Guid.Empty
                };

                var project = new ProjectModel
                {
                    Id = projectUpload.Id,
                    Title = projectUpload.Title,
                    Description = projectUpload.Description,
                    PortfolioProfileId = _userService.PortfolioProfileId ?? Guid.Empty,
                    FileId = projectFile.Id,
                    ProjectFile = projectFile,
                    ProjectPhotos = [.. photos.Select(photo => new ProjectPhotoModel { Photo = photo })],
                    ProjectVideos = [.. videos.Select(video => new ProjectVideoModel { Video = video })],
                    IsHidden = projectUpload.IsHidden,
                };

                _context.Projects.Add(project);

                return ResultResponse<ProjectModel>.Success(project);
            }
            catch (AppException ex)
            {
                return ResultResponse<ProjectModel>.Failure(
                   new Error(ex.StatusCode, ex.Message)
               );
            }

        }

        public async Task<ResultResponse<ProjectModel>> DeleteProject(
            Guid projectId)
        {
            var query = await GetProject(projectId);
            var project = query.Result;

            if (project == null)
            {
                return ResultResponse<ProjectModel>.Failure(
                    new Error(
                        HttpStatusCode.NotFound,
                        "No project found with the given id"));
            }

            _context.Projects.Remove(project);

            return ResultResponse<ProjectModel>.Success(project);

        }

        public async Task<ResultResponse<ProjectModel?>> GetProject(
            Guid projectId)
        {
            try
            {
                var response = await GetProjectQuery()
                    .FirstOrDefaultAsync(m => m.Id == projectId);

                return ResultResponse<ProjectModel?>.Success(response);
            }
            catch (Exception ex)
            {
                return ResultResponse<ProjectModel?>.Failure(
                    new Error(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        public async Task<ResultResponse<List<ProjectModel>>> GetAllProject(string username)
        {
            try
            {
                var result = await GetProjectQuery()
                    .Where(p => p.PortfolioProfile!.User.UserName == username)
                    .ToListAsync();

                return ResultResponse<List<ProjectModel>>.Success(result);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<ProjectModel>>.Failure(
                    new Error(HttpStatusCode.BadRequest, ex.Message));
            }
        }



        public async Task<ResultResponse<ProjectModel>> UpdateProject(
            ProjectUpload request,
            FileModel? file,
            List<VideoModel> videos,
            List<PhotoModel> photos,
            ProjectModel project)
        {
            try
            {
                if (request.DeletedIds.Any())
                {
                    // Handle deleted photos
                    var photosToRemove = project.ProjectPhotos
                        .Where(pp => pp.Photo != null && request.DeletedIds.Contains(pp.Photo.Id.ToString()))
                        .ToList();

                    foreach (var pp in photosToRemove)
                    {
                        project.ProjectPhotos.Remove(pp);
                        _context.Photos.Remove(pp.Photo);
                    }

                    // Handle deleted videos
                    var videosToRemove = project.ProjectVideos
                        .Where(pv => pv.Video != null && request.DeletedIds.Contains(pv.Video.Id.ToString()))
                        .ToList();

                    foreach (var pv in videosToRemove)
                    {
                        project.ProjectVideos.Remove(pv);
                        _context.Videos.Remove(pv.Video);
                    }

                    if (project.ProjectFile != null && 
                        request.DeletedIds.Contains(project.ProjectFile.Id.ToString()))
                    {
                        _context.Files.Remove(project.ProjectFile);
                        project.ProjectFile = null;
                    }
                }

                //Start replace
                project.Title = request.Title;
                project.Description = request.Description;
                project.IsHidden = request.IsHidden;

                // Add new photos
                project.ProjectPhotos.AddRange(
                    photos.Select(photo =>
                    {
                        _context.Photos.Add(photo);
                        return new ProjectPhotoModel { Photo = photo };
                    })
                );

                // Add new videos
                project.ProjectVideos.AddRange(
                    videos.Select(video =>
                    {
                        _context.Videos.Add(video);
                        return new ProjectVideoModel { Video = video };
                    })
                );

                // Handle file — always replace if new one uploaded
                if (file != null)
                {
                    if (project.ProjectFile != null)
                    {
                        _context.Files.Remove(project.ProjectFile);
                    }
                    
                    _context.Files.Add(file);
                    project.ProjectFile = file;
                }

                return ResultResponse<ProjectModel>.Success(project);
            }

            catch (Exception ex) {
                return ResultResponse<ProjectModel>.Failure(
                   new Error(System.Net.HttpStatusCode.Conflict, ex.Message)
               );
            }
            
        }

        private IQueryable<ProjectModel> GetProjectQuery()
        {
            return _context.Projects
                .Include(p => p.ProjectPhotos)
                    .ThenInclude(p => p.Photo)
                .Include(p => p.ProjectVideos)
                    .ThenInclude(v => v.Video)
                .Include(f => f.ProjectFile)
                .Include(p => p.PortfolioProfile)
                    .ThenInclude(pp => pp.User);
        }

    }
}
