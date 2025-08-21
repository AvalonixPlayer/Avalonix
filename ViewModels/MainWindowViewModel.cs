using Avalonix.Services;
using Microsoft.Extensions.Logging;

namespace Avalonix.ViewModels;

public class MainWindowViewModel(ILogger<MainWindowViewModel> logger, IWindowManager windowManager) : ViewModelBase
{
    public void Exit()
    {
        logger.LogInformation("Exit button clicked");
        windowManager.CloseMainWindow(); 
    } 
}
