using System.Threading.Tasks;
using Avalonix.Services.SettingsManager;

namespace Avalonix.ViewModel.Settings;

public class SettingsWindowViewModel(ISettingsManager manager) : ViewModelBase, ISettingsWindowViewModel
{
    public async Task SaveSettingsAsync(Model.UserSettings.Settings settings) =>
        await manager.SaveSettings();

    public Task<Model.UserSettings.Settings?> GetSettingsAsync() =>
        Task.FromResult(manager.Settings);
}
