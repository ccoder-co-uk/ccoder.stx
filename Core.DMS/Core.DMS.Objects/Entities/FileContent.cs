using System;

namespace Core.DMS.Objects.Entities
{
    public class FileContent
    {
        public Guid Id { get; set; }
        public Guid FileId { get; set; }
        public string Description { get; set; }
        public string Size { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public int Version { get; set; }
        public byte[] RawData { get; set; }
    }
}