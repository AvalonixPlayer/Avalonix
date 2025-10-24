using System.Threading.Tasks;
using Avalonix.Models.UserSettings;

namespace Avalonix.Services.SettingsManager;

public interface ISettingsManager
{
    Task SaveSettings(Settings settings);
    Task<Settings> GetSettings();
}