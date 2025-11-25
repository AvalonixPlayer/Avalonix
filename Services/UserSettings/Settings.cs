using Avalonix.Services.UserSettings.AvalonixSettingsFiles;

namespace Avalonix.Services.UserSettings;

public record Settings
{
    public AvalonixSettings Avalonix { get; set; } = new();
}