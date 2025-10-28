using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonix.Models.Media.Track;
using Avalonix.ViewModels.Strategy;

namespace Avalonix.ViewModels;

public interface IPlaylistEditOrCreateWindowViewModel
{
    ISecondWindowStrategy Strategy { get; }
    Task<List<string>?> OpenTrackFileDialogAsync(Window parent);
    Task ExecuteAsync(string playlistName, List<Track> tracksPaths);
}