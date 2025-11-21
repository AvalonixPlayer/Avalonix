using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Models.Media;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.PlayableManager;
using Avalonix.ViewModels.Strategy;

namespace Avalonix.ViewModels.PlayableSelectViewModel;

public class PlayableSelectViewModel(IPlayableManager playableItemsManager, IPlayableWindowStrategy strategy) : ViewModelBase, IPlayableSelectViewModel
{
    public IPlayableWindowStrategy Strategy { get; } = strategy;
    public async Task<List<IPlayable>> GetPlayableItems()
        => (await playableItemsManager.GetPlayableItems()).ToList();

    public List<IPlayable> SearchItem(string text, List<IPlayable> playable) =>
        string.IsNullOrWhiteSpace(text) ? playable : playable.
            Where(item => item.Name.
                Contains(text, StringComparison.CurrentCultureIgnoreCase)).ToList();

    public async Task ExecuteAction(IPlayable playable) => await Strategy.ExecuteAsync(playable);
}
