using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;

namespace Avalonix.Views;

public partial class MainWindow  : Window 
{
    private readonly ILogger<MainWindow> _logger;

    public MainWindow(ILogger<MainWindow> logger, MainWindowViewModel vm)
    {
        _logger = logger;
        InitializeComponent();
        _logger.LogInformation("MainWindow initialized");
    }

    private void InitializeComponent() =>
        AvaloniaXamlLoader.Load(this);
}