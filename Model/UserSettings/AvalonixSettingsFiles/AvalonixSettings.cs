using System;
using System.Text.Json.Serialization;

namespace Avalonix.Model.UserSettings.AvalonixSettingsFiles;

public record AvalonixSettings
{
    [JsonIgnore] public Action<bool>? LoopChanged;
    [JsonIgnore] public Action<bool>? ShuffleChanged;
    public readonly PlaySettings PlaySettings = new();
    public uint Volume = 100;
    public string? MusicFilesPath { get; set; }
}
