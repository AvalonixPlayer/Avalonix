using System.Threading.Tasks;
using Avalonix.Models.Media.Track;
using Avalonix.Views.SecondaryWindows.AboutWindow;
using Avalonix.Views.SecondaryWindows.EditMetadataWindow;
using Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;
using Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;
using Avalonix.Views.SecondaryWindows.ShowTrackWindow;

namespace Avalonix.Services.WindowManager;

public interface IWindowManager
{
    Task CloseMainWindowAsync();
    PlaylistCreateWindow PlaylistCreateWindow_Open();
    PlaylistSelectWindow PlaylistSelectWindow_Open();
    AboutWindow AboutWindow_Open();
    ShowTrackWindow ShowTrackWindow_Open(Track track);
    EditMetadataWindow EditMetadataWindow_Open(Track track);
    PlaylistSelectWindow PlaylistDeleteWindow_Open();
}