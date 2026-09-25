using DiaryPortfolio.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiaryPortfolio.Domain.Entities
{
    public class ProjectModel : IUserOwner
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        //FK, EF
        public Guid PortfolioProfileId { get; set; }
        public PortfolioProfileModel? PortfolioProfile { get; set; }
        public Guid? FileId { get; set; }
        public FileModel? ProjectFile { get; set; }
        public List<ProjectPhotoModel> ProjectPhotos { get; set; } = [];
        public List<ProjectVideoModel> ProjectVideos { get; set; } = [];
        public Guid? ProjectTypeId { get; set; }
        public ProjectTypeModel? ProjectType { get; set; }

        public bool IsHidden { get; set; } = false;

        public Guid OwnerId => PortfolioProfile?.UserId ?? Guid.Empty;
    }
}
