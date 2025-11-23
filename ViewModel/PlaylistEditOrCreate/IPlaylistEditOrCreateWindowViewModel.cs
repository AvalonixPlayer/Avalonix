using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonix.Model.Media.Track;
using Avalonix.ViewModel.Strategy;

namespace Avalonix.ViewModel.PlaylistEditOrCreate;

public interface IPlaylistEditOrCreateWindowViewModel
{
    IPlayableWindowStrategy Strategy { get; }
    Task<List<string>?> OpenTrackFileDialogAsync(Window parent);
    Task<string?> OpenObservingDirectoryDialogAsync(Window parent);
    Task ExecuteAsync(string playlistName, List<Track> tracksPaths, string? observingDirectory);
}
