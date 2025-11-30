using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonix.ViewModel.Strategy;

namespace Avalonix.ViewModel.PlaylistEditOrCreate;

public interface IPlaylistEditOrCreateWindowViewModel
{
    IPlayableWindowStrategy Strategy { get; }
    Task<List<string>?> OpenTrackFileDialogAsync(Window parent);
    Task<string?> OpenObservingDirectoryDialogAsync(Window parent);
    Task ExecuteAsync(string playlistName, List<string> tracksPaths, string? observingDirectory);
}