using System;
using System.Text.Json.Serialization;

namespace Avalonix.Services.UserSettings.AvalonixSettingsFiles;

public record AvalonixSettings
{
    [JsonIgnore] public Action<bool>? LoopChanged;
    [JsonIgnore] public Action<bool>? SuffleChanged;
    public PlaySettings PlaySettings { get; set; } = new();
    public uint Volume { get; set; } = 100;
}