using System;
using System.Text.Json.Serialization;

namespace Avalonix.Model.UserSettings.AvalonixSettingsFiles;

public record AvalonixSettings
{
    [JsonIgnore] public Action<bool>? LoopChanged;
    [JsonIgnore] public Action<bool>? ShuffleChanged;
    [JsonInclude] public PlaySettings PlaySettings = new();
    [JsonInclude] public uint Volume = 100;
    [JsonInclude] public string? MusicFilesPath { get; set; }
}