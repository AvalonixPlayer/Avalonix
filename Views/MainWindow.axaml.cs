using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;
using static Avalonix.ViewModels.MainWindowViewModel;

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

    private void ExitButton_OnClick(object? sender, RoutedEventArgs e) => MainWindowViewModel.Exit();
}