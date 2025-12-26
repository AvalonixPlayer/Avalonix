using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Avalonix.Model.UserSettings.AvalonixSettingsFiles;

public record AvalonixSettings
{
    [JsonIgnore] public Action<bool>? LoopChanged;
    [JsonInclude] public PlaySettings PlaySettings = new();
    [JsonInclude] public EqualizerSettings EqualizerSettings = new();
    [JsonIgnore] public Action<bool>? ShuffleChanged;
    [JsonInclude] public uint Volume = 100;
    [JsonInclude] public List<string> MusicFilesPaths { get; set; } = [];
    [JsonInclude] public string? AutoAlbumCoverPath { get; set; }
}