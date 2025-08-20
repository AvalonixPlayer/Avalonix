using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.Logging;

namespace Avalonix.Views;

public partial class MainWindow  : Window 
{
    private readonly ILogger<MainWindow> _logger;
    public MainWindow(ILogger<MainWindow> logger)
    {
        _logger = logger; 
        InitializeComponent();
        _logger.LogInformation("MainWindow initialized");
    }

    private void ExitApp_OnClick(object? sender, RoutedEventArgs e)
    {
        _logger.LogInformation("Exiting app");
        Close();
    }
}