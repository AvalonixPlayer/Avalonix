using System.Threading.Tasks;

namespace Avalonix.Services.VersionManager;

public interface IVersionManager
{
    Release CurrentRelease { get; }
    Task<Release> GetLastRelease();
}