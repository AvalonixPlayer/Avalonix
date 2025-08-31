using System.Text.Json.Serialization;
using Avalonia.Media;

namespace Avalonix.Models.UserSettings.Theme.Components;

public class Buttons : IThemeComponent
{
    [JsonConstructor]
    public Buttons() {}
    public string ButtonBackground { get; set; } = "#FF008000";
}