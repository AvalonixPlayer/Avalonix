using System;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace Avalonix.Services.ThemeManager;

public class ThemeManager : IThemeManager 
{
    private Styles _styles = [];
    // public List<Styles> AvailableThemes = []; in future
    
    public Styles LoadTheme(string themeName)
    {
        _styles.Clear();
        
        var themeInclude = new StyleInclude(new Uri("avares://YourApp/"))
        {
            Source = new Uri($"avares://Avalonix/Styles/{themeName}.xaml")
        };
        
        _styles.Add(themeInclude);
        return _styles;
    }
    
    public Styles ResetTheme() => LoadTheme("Purple");
}