using Microsoft.Extensions.Logging;

namespace Avalonix.ViewModels;

public class MainWindowViewModel(ILogger logger) : ViewModelBase
{
    public void HelpCommand() => logger.LogDebug("Help");
}
