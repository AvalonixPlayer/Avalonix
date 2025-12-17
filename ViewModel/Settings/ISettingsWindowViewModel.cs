using System.Threading.Tasks;
using Avalonia.Controls;

namespace Avalonix.ViewModel.Settings;

public interface ISettingsWindowViewModel
{
    Task<string?> OpenCoverFileDialogAsync(Window parent);
    Task SaveSettingsAsync(Model.UserSettings.Settings settings);
    Task<Model.UserSettings.Settings?> GetSettingsAsync();
}
