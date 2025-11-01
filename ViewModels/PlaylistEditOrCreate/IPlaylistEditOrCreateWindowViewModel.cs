using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonix.Models.Media.Track;
using Avalonix.ViewModels.Strategy;

namespace Avalonix.ViewModels.PlaylistEditOrCreate;

public interface IPlaylistEditOrCreateWindowViewModel
{
    ISecondWindowStrategy Strategy { get; }
    Task<List<string>?> OpenTrackFileDialogAsync(Window parent);
    Task<string?> OpenObservingDirectoryDialogAsync(Window parent);
    Task ExecuteAsync(string playlistName, List<Track> tracksPaths, string? observingDirectory);
}