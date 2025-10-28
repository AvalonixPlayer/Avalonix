using System.Threading.Tasks;
using Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;
using Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;

namespace Avalonix.Services.WindowManager;

public interface IWindowManager
{
    Task CloseMainWindowAsync();
    Task<PlaylistCreateWindow> PlaylistCreateWindow_Open();
    Task<PlaylistSelectWindow> PlaylistSelectWindow_Open();
}