using System.Text.Json.Serialization;
using Avalonix.Models.UserSettings.AvalonixSettingsFiles;

namespace Avalonix.Models.UserSettings;

public class Settings
{
    [JsonIgnore] public Theme.Theme Theme { get; set; }
    public static AvalonixSettings Avalonix { get; set; } = new AvalonixSettings();
}