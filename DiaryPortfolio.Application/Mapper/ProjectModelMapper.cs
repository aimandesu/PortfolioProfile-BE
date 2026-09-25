using DiaryPortfolio.Application.DTOs;
using DiaryPortfolio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiaryPortfolio.Application.Mapper
{
    static internal class ProjectModelMapper
    {
        public static ProjectModelDto ToProjectModelDto(this ProjectModel projectModel)
        {
            return new ProjectModelDto
            {
                Id = projectModel.Id,
                Title = projectModel.Title,
                Description = projectModel.Description,
                ProjectFile = projectModel.ProjectFile,
                ProjectPhotos = [.. projectModel.ProjectPhotos
                    .Select(e => e.Photo)
                    .OfType<PhotoModel>()],
                ProjectVideos = [.. projectModel.ProjectVideos
                    .Select(e => e.Video)
                    .OfType<VideoModel>()],
                IsHidden = projectModel.IsHidden,
            };
        }
    }
}
