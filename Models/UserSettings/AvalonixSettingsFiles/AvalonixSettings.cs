using System;
using System.Text.Json.Serialization;

namespace Avalonix.Models.UserSettings.AvalonixSettingsFiles;

public record AvalonixSettings
{
    [JsonIgnore] public Action<bool>? SuffleChanged;
    [JsonIgnore] public Action<bool>? LoopChanged;
    public PlaySettings PlaySettings { get; set; } = new();
    public uint Volume { get; set; } = 100;
}