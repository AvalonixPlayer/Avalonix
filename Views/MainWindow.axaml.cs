using Avalonia;
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
        SongsListBox.Items.Add(new ListBoxItem
        {
            Content = "Ls",
            CornerRadius = new CornerRadius(5)
        });
    }
}