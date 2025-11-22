using System.Text.Json.Serialization;

namespace Avalonix.Services.UserSettings.Theme.Components;

public class Buttons : IThemeComponent
{
    [JsonConstructor]
    public Buttons() {}
    public string ButtonBackground { get; set; } = "#FF008000";
}