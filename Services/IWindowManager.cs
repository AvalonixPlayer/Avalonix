using System.Threading.Tasks;

namespace Avalonix.Services;

public interface IWindowManager
{
    Task CloseMainWindowAsync();
}