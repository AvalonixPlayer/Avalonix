using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonix.Models.Media.Track;

namespace Avalonix.ViewModels;

public interface IPlaylistEditOrCreateWindowViewModel
{
    Task<List<string>?> OpenTrackFileDialogAsync(Window parent);
    Task ExecuteAsync(string playlistName, List<Track> tracksPaths);
}