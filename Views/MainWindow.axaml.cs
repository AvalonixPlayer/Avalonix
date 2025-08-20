using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;

namespace Avalonix.Views;

public partial class MainWindow : Window 
{
    public MainWindow(ILogger logger) 
    {
        InitializeComponent();
        logger.LogInformation("MainWindow initialized");
        SongsListBox.Items.Add(new ListBoxItem
        {
            Content = "Ls",
            CornerRadius = new CornerRadius(5)
        });
    }
}