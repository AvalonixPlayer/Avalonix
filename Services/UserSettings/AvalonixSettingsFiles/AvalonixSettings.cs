using System;
using System.Text.Json.Serialization;

namespace Avalonix.Services.UserSettings.AvalonixSettingsFiles;

public record AvalonixSettings
{
    [JsonIgnore] public Action<bool>? LoopChanged;
    [JsonIgnore] public Action<bool>? SuffleChanged;
    public PlaySettings PlaySettings = new();
    public uint Volume = 100;
    public string? MusicFilesPath;
}