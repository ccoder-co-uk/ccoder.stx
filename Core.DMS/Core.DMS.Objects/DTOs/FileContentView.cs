namespace Core.DMS.Objects.DTOs
{
    public class FileContentView
    {
        public byte[] RawData { get; set; }
        public string MimeType { get; set; }
        public string Path { get; set; }
    }
}
