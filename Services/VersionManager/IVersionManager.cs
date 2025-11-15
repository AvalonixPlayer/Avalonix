using System.Threading.Tasks;

namespace Avalonix.Services.VersionManager;

public interface IVersionManager
{
    Task<Release> GetLastRelease();
    Task<Release> GetCurrentRelease();
}