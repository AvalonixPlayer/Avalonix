using System.Text.Json.Serialization;
using Avalonix.Model.UserSettings.Theme.Components;

namespace Avalonix.Model.UserSettings.Theme;

public record Theme
{
    [JsonConstructor]
    public Theme()
    {
    }

    public required string Name { get; init; }
    public Buttons Buttons { get; set; } = new();
    public SecondaryBackground SecondaryBackground { get; set; } = new();
}