using System;
using Avalonia;
using Avalonia.Controls;
using NeoSimpleLogger;

namespace Avalonix;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        if (Program.Logger != null) Program.Logger.Info("MainWindow opened");
        InitializeComponent();
    }

    private void AvaloniaObject_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
       // Изменение громкости 
    }
}
