using System.Threading.Tasks;
using Avalonix.SecondaryWindows.PlaylistCreateWindow;

namespace Avalonix.Services;

public interface IWindowManager
{
    Task CloseMainWindowAsync();
    Task<PlaylistCreateWindow> PlaylistCreateWindow_Open();
}