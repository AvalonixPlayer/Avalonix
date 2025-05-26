using Avalonia.Controls;
using NeoSimpleLogger;

namespace Avalonix;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        var logger = new Logger();
        InitializeComponent();
        logger.Info("App Started");
    }
}
