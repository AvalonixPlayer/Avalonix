using System.Threading.Tasks;
using Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;
using Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;

namespace Avalonix.ViewModels;

public interface IMainWindowViewModel
{
    Task ExitAsync();
    Task<PlaylistCreateWindow> PlaylistCreateWindow_Open();
    Task<PlaylistSelectWindow> PlaylistSelectWindow_Open();
}