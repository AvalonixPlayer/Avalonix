using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Model.Media;
using Avalonix.Services.PlayableManager;
using Avalonix.ViewModel.Strategy;

namespace Avalonix.ViewModel.PlayableSelectViewModel;

public class PlayableSelectViewModel(IPlayableManager playableItemsManager, IPlayableWindowStrategy strategy) : ViewModelBase, IPlayableSelectViewModel
{
    public IPlayableWindowStrategy Strategy { get; } = strategy;
    public async Task<List<IPlayable>> GetPlayableItems()
        => (await playableItemsManager.GetPlayables()).ToList();

    public List<IPlayable> SearchItem(string text, List<IPlayable> playable) =>
        string.IsNullOrWhiteSpace(text) ? playable : playable.
            Where(item => item.Name.
                Contains(text, StringComparison.CurrentCultureIgnoreCase)).ToList();

    public async Task ExecuteAction(IPlayable playable) => await Strategy.ExecuteAsync(playable);
}
