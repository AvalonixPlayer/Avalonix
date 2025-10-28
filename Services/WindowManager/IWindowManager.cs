using System.Threading.Tasks;
using Avalonix.Views.SecondaryWindows.AboutWindow;
using Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;
using Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;

namespace Avalonix.Services.WindowManager;

public interface IWindowManager
{
    Task CloseMainWindowAsync();
    PlaylistCreateWindow PlaylistCreateWindow_Open();
    PlaylistSelectWindow PlaylistSelectWindow_Open();
    AboutWindow AboutWindow_Open();
}