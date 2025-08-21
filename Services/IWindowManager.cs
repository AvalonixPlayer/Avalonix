using System.Threading.Tasks;

namespace Avalonix.Services;

public interface IWindowManager
{
    void CloseMainWindow();
    Task CloseMainWindowAsync();
}