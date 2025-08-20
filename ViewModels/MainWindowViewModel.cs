using Microsoft.Extensions.Logging;

namespace Avalonix.ViewModels;

public class MainWindowViewModel  
{
    private readonly ILogger<MainWindowViewModel> _logger;
    public MainWindowViewModel(ILogger<MainWindowViewModel> logger)
    {
        _logger = logger;     
    }
    
    
}
