using System;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Services;
using Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;
using Microsoft.Extensions.Logging;

namespace Avalonix.ViewModels;

public class MainWindowViewModel(ILogger<MainWindowViewModel> logger, IWindowManager windowManager, DiskManager diskManager) 
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

    public async Task<PlaylistCreateWindow> PlaylistCreateWindow_Open()
    {
        try
        {
            return await windowManager.PlaylistCreateWindow_Open();   
        }
        catch (Exception e)
        {  
            logger.LogError("Error while exiting app: {e}", e.Message);
            throw new Exception("Error while exiting app");
        }
    }
}