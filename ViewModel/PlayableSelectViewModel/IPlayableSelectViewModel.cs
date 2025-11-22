using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Services.Media;
using Avalonix.ViewModels.Strategy;

namespace Avalonix.ViewModels.PlayableSelectViewModel;

public interface IPlayableSelectViewModel
{
    Task<List<IPlayable>> GetPlayableItems();
    List<IPlayable> SearchItem(string text, List<IPlayable> playable);
    Task ExecuteAction(IPlayable playable);
    IPlayableWindowStrategy Strategy { get; }
}
