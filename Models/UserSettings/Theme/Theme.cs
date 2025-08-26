using Avalonix.Models.UserSettings.Theme.Components;

namespace Avalonix.Models.UserSettings.Theme;

public struct Theme()
{
    public IThemeComponent Buttons { get; set; } = new Buttons();
    public IThemeComponent SecondaryBackground { get; set; } = new SecondaryBackground();
}