using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Media;
using Avalonix.ViewModels.Strategy;

namespace Avalonix.ViewModels.PlayableSelectViewModel;

public interface IPlayableSelectViewModel
{
    Task<List<IPlayable>> GetPlaylists();
    List<IPlayable> SearchItem(string text, List<IPlayable> playlists);
    Task ExecuteAction(IPlayable playlist);
    IPlayableWindowStrategy Strategy { get; }
}
