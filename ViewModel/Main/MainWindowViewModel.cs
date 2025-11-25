using System;
using System.Threading.Tasks;
using Avalonix.Services.WindowManager;
using Avalonix.View.SecondaryWindows.PlayableSelectWindow;
using Avalonix.View.SecondaryWindows.PlaylistCreateWindow;
using Microsoft.Extensions.Logging;

namespace Avalonix.ViewModel.Main;

public class MainWindowViewModel(ILogger<MainWindowViewModel> logger, IWindowManager windowManager)
    : ViewModelBase, IMainWindowViewModel
{
    public async Task ExitAsync()
    {
        try
        {
            await windowManager.CloseMainWindowAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("Error while exiting app: {e}", ex.Message);
        }
    }

    public PlaylistCreateWindow PlaylistCreateWindow_Open()
    {
        try
        {
            return windowManager.PlaylistCreateWindow_Open();
        }
        catch (Exception ex)
        {
            logger.LogError("Error while opening PlaylistCreateWindow: {e}", ex.Message);
            return null!;
        }
    }

    public PlayableSelectWindow PlaylistSelectWindow_Open()
    {
        try
        {
            return windowManager.PlaylistSelectWindow_Open();
        }
        catch (Exception ex)
        {
            logger.LogError("Error while opening PlaylistSelectWindow: {e}", ex.Message);
            return null!;
        }
    }

    public PlayableSelectWindow AlbumSelectWindow_Open()
    {
        try
        {
            return windowManager.AlbumSelectAndPlayWindow_Open();
        }
        catch (Exception ex)
        {
            logger.LogError("Error while opening PlaylistSelectWindow: {e}", ex.Message);
            return null!;
        }
    }
}