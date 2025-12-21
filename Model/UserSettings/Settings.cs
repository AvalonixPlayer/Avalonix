using Avalonix.Model.UserSettings.AvalonixSettingsFiles;

namespace Avalonix.Model.UserSettings;

public record Settings
{
    public AvalonixSettings Avalonix { get; set; } = new();
}