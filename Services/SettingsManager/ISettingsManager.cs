using System.Threading.Tasks;
using Avalonix.Services.UserSettings;

namespace Avalonix.Services.SettingsManager;

public interface ISettingsManager
{
    public Settings? Settings { get; }
    Task SaveSettingsAsync();
}