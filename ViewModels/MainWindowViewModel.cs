using System.Threading.Tasks;
using Avalonix.Services;
using Microsoft.Extensions.Logging;

namespace Avalonix.ViewModels;

public class MainWindowViewModel(ILogger<MainWindowViewModel> logger, IWindowManager windowManager)
    : ViewModelBase
{
    public async Task ExitButton_OnClick()
    {
        logger.LogInformation("Exit button clicked");
        windowManager.CloseMainWindow(); 
    }
}
