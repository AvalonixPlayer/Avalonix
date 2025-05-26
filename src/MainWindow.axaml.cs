using Avalonia.Controls;
using NeoSimpleLogger.Logger;

namespace Avalonix;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Info("App started");
    }
}
