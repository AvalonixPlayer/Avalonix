using Avalonia.Media;

namespace Avalonix.Models.UserSettings.Theme.Components;

public struct SecondaryBackground() : IThemeComponent
{
    public Color ButtonBackground { get; set; } = Colors.Gray;
}