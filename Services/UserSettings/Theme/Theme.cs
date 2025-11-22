using System.Text.Json.Serialization;
using Avalonix.Services.UserSettings.Theme.Components;

namespace Avalonix.Services.UserSettings.Theme;

public class Theme
{
    [JsonConstructor]
    public Theme() {}
    public required string Name { get; init; }
    public Buttons Buttons { get; set; } = new();
    public SecondaryBackground SecondaryBackground { get; set; } = new();
}