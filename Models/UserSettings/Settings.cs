using Avalonix.Models.UserSettings.AvalonixSettingsFiles;

namespace Avalonix.Models.UserSettings;

public class Settings
{
    public Theme.Theme Theme { get; set; }
    public AvalonixSettings Avalonix { get; set; }
}