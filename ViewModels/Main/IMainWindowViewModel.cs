using System.Threading.Tasks;
using Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;
using Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;

namespace Avalonix.ViewModels;

public interface IMainWindowViewModel
{
    Task ExitAsync();
    PlaylistCreateWindow PlaylistCreateWindow_Open();
    PlaylistSelectWindow PlaylistSelectWindow_Open();
}