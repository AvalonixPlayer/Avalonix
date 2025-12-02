using System.Threading.Tasks;

namespace Avalonix.ViewModel.Settings;

public interface ISettingsWindowViewModel
{
    Task SaveSettingsAsync(Model.UserSettings.Settings settings);
    Task<Model.UserSettings.Settings?> GetSettingsAsync();
}
