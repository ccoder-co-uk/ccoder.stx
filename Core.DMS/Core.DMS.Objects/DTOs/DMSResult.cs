using System.IO;

namespace Core.DMS.Objects.DTOs
{
    public class DMSResult
    {
        public string MimeType { get; set; }
        public Stream Data { get; set; }
    }
}
