using Avalonia.Styling;

namespace Avalonix.Services.ThemeManager;

public interface IThemeManager
{
    Styles ResetTheme();
    Styles LoadTheme(string themeName);
}