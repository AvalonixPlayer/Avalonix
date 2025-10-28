using Avalonix.Models.UserSettings.AvalonixSettingsFiles;

namespace Avalonix.Models.UserSettings;

public record Settings
{
    public AvalonixSettings Avalonix { get; set; } = new ();
}