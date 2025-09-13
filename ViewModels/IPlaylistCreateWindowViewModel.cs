using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Avalonix.ViewModels;

public interface IPlaylistCreateWindowViewModel
{
    Task<string[]?> OpenTrackFileDialogAsync(Window parent);
    Task CreatePlaylistAsync(string playlistName, List<string> tracksPaths);
}