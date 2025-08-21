using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services;

public class WindowManager(ILogger<WindowManager> logger) : IWindowManager
{
    public void CloseMainWindow()
    {
        logger.LogInformation("WindowManager: Closing app");
        
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        } 
        // Another variant
        // Environment.Exit(0);
    }
}