using System.Threading.Tasks;
using Avalonix.Model.UserSettings;

namespace Avalonix.Services.SettingsManager;

public interface ISettingsManager
{
    public Settings? Settings { get; set; }
    Task SaveSettings();
}