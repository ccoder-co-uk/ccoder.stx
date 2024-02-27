using System;

namespace Core.DMS.Objects.Entities
{
    public class File
    {
        public Guid Id { get; set; }

        public Guid FolderId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Path { get; set; }

        public string MimeType { get; set; }

        public string CreatedBy { get; set; }

        public string Size { get; set; }

        public DateTimeOffset CreatedOn { get; set; }
    }
}