using System.Threading.Tasks;
using Avalonix.Services.Media.Track;
using Avalonix.View.SecondaryWindows.AboutWindow;
using Avalonix.View.SecondaryWindows.EditMetadataWindow;
using Avalonix.View.SecondaryWindows.PlayableSelectWindow;
using Avalonix.View.SecondaryWindows.PlaylistCreateWindow;
using Avalonix.View.SecondaryWindows.ShowTrackWindow;

namespace Avalonix.Services.WindowManager;

public interface IWindowManager
{
    Task CloseMainWindowAsync();
    PlaylistCreateWindow PlaylistCreateWindow_Open();
    PlayableSelectWindow PlaylistSelectWindow_Open();
    AboutWindow AboutWindow_Open();
    ShowTrackWindow ShowTrackWindow_Open(Track track);
    EditMetadataWindow EditMetadataWindow_Open(Track track);
    PlayableSelectWindow PlaylistDeleteWindow_Open();
    PlayableSelectWindow AlbumSelectAndPlayWindow_Open();
}
