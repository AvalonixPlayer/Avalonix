using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace Avalonix.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly ILogger _logger;
    
    public MainWindowViewModel(ILogger logger)
    {
        _logger = logger;
        
        HelpCommand = ReactiveCommand.CreateFromTask(async () => 
        {
            _logger.LogInformation("Help Button Click");
            await Task.CompletedTask;
        }, Observable.Return(true));
        
        ExitCommand = ReactiveCommand.Create(() => 
        {
            _logger.LogInformation("Exit Button Click");
            Environment.Exit(0);
        }, Observable.Return(true));
    }
    
    public ReactiveCommand<Unit, Unit> HelpCommand { get; }
    public ReactiveCommand<Unit, Unit> ExitCommand { get; }
}
