namespace Avalonix.Models.UserSettings.AvalonixSettingsFiles;

public class AvalonixSettings
{
    public Playlists Playlists { get; set; } = new();
    public uint Volume { get; set; } = 100;
}