using System.Reactive;
using System.Windows.Input;
using Avalonix.Services;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace Avalonix.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly IWindowManager _windowManager;

    public MainWindowViewModel(ILogger<MainWindowViewModel> logger, IWindowManager windowManager)
    {
        _logger = logger;
        _windowManager = windowManager;

        ExitAppCommand = ReactiveCommand.Create(() =>
        {
            _logger.LogInformation("Exiting app");
            _windowManager.CloseMainWindow();
            return Unit.Default;
        });
    }

    public ICommand ExitAppCommand { get; }
}
