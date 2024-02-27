using Core.DMS.Objects;
using Core.DMS.Objects.DTOs;
using System.IO;
using System.Threading.Tasks;
using Path = Core.DMS.Objects.Path;

namespace Core.DMS.Services.Orchestration
{
    public interface IDMSResultOrchestrationService
    {
        Task<DMSResult> Get(Path path, int version = 0);
        Task Save(Path path, MemoryStream content = null);
        Task Move(Path oldPath, Path newPath);
    }
}