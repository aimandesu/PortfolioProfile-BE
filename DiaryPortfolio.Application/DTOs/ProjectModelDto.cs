using DiaryPortfolio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiaryPortfolio.Application.DTOs
{
    public class ProjectModelDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public FileModel? ProjectFile { get; set; }
        public List<PhotoModel> ProjectPhotos { get; set; } = [];
        public List<VideoModel> ProjectVideos { get; set; } = [];
        public bool IsHidden { get; set; } = false;
    }
}
