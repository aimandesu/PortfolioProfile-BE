using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiaryPortfolio.Application.Request
{
    public class ProjectUpload
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        //public MediaStream? ProjectFileStream { get; set; }
        //public List<MediaStream> MediaFileStreams { get; set; } = [];
        public List<string> DeletedIds { get; set; } = [];
        public bool IsHidden { get; set; } = false;
    }
}
