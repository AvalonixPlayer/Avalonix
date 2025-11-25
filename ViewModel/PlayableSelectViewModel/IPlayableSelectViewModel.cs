using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Model.Media;
using Avalonix.ViewModel.Strategy;

namespace Avalonix.ViewModel.PlayableSelectViewModel;

public interface IPlayableSelectViewModel
{
    IPlayableWindowStrategy Strategy { get; }
    Task<List<IPlayable>> GetPlayableItems();
    List<IPlayable> SearchItem(string text, List<IPlayable> playable);
    Task ExecuteAction(IPlayable playable);
}