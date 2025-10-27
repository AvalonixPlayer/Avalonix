using System.Text.Json.Serialization;
using Avalonix.Models.UserSettings.AvalonixSettingsFiles;

namespace Avalonix.Models.UserSettings;

public record Settings
{
    [JsonIgnore] public Theme.Theme Theme { get; set; }
    public AvalonixSettings Avalonix { get; set; } = new ();
}