namespace Avalonix.Models.UserSettings.AvalonixSettingsFiles;

public class AvalonixSettings
{
    public PlaySettings PlaySettings { get; set; } = new();
    public uint Volume { get; set; } = 100;
}