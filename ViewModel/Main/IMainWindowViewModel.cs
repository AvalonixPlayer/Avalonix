using Avalonix.View.SecondaryWindows.PlayableSelectWindow;
using Avalonix.View.SecondaryWindows.PlaylistCreateWindow;

namespace Avalonix.ViewModel.Main;

public interface IMainWindowViewModel
{
    PlaylistCreateWindow PlaylistCreateWindow_Open();
    PlayableSelectWindow PlaylistSelectWindow_Open();
    PlayableSelectWindow AlbumSelectWindow_Open();
}