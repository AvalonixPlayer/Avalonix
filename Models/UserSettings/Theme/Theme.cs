using System.Text.Json.Serialization;
using Avalonix.Models.UserSettings.Theme.Components;

namespace Avalonix.Models.UserSettings.Theme;

public class Theme
{
    [JsonConstructor]
    public Theme() {}
    public string Name { get; set; }
    public Buttons Buttons { get; set; } = new();
    public SecondaryBackground SecondaryBackground { get; set; } = new();
}