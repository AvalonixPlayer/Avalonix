namespace Avalonix.Models.UserSettings.AvalonixSettingsFiles;

public class AvalonixSettings
{
    public Playlists Playlists { get; set; } = new();
    public int Volume { get; set; } = 100;
}