using Avalonix.Services;
using Microsoft.Extensions.Logging;

namespace Avalonix.ViewModels;

public class MainWindowViewModel(ILogger<MainWindowViewModel> logger, IWindowManager windowManager) 
    : ViewModelBase
{
    // Use in future
    //private async Task ExitAsync() => await windowManager.CloseMainWindowAsync();

}