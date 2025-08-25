using System;
using System.Threading.Tasks;
using Avalonix.Services;
using Microsoft.Extensions.Logging;

namespace Avalonix.ViewModels;

public class MainWindowViewModel(ILogger<MainWindowViewModel> logger, IWindowManager windowManager) 
    : ViewModelBase
{
    public async Task ExitAsync()
    {
        try
        {
            await windowManager.CloseMainWindowAsync();
        }
        catch (Exception e)
        {
            logger.LogError("Error while exiting app: {e}", e.Message);
        }
    }

    public async Task PlaylistCreateWindow_Open()
    {
        try
        {
            windowManager.PlaylistCreateWindow_Open();   
        }
        catch (Exception e)
        {  
            logger.LogError("Error while exiting app: {e}", e.Message);
        }
    }
}