using System;

namespace Core.DMS.Objects.Entities
{
    public class Folder
    {
        public Guid Id { get; set; }
        public int AppId { get; set; }
        public Guid? ParentId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}