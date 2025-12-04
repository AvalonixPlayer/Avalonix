using System.Threading.Tasks;
using Avalonix.Services.SettingsManager;

namespace Avalonix.ViewModel.Settings;

public class SettingsWindowViewModel(ISettingsManager manager) : ViewModelBase, ISettingsWindowViewModel
{
    public async Task SaveSettingsAsync(Model.UserSettings.Settings settings)
    {
        manager.Settings = settings;
        await manager.SaveSettingsAsync();
    }

    public Task<Model.UserSettings.Settings?> GetSettingsAsync() =>
        Task.FromResult(manager.Settings);
}
