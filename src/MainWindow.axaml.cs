using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using static Avalonix.Program;

namespace Avalonix;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        Logger.Info("MainWindow opened");
        InitializeComponent();
    }

    private void AvaloniaObject_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        // Изменение громкости 
    }

    private void BackButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Logger.Info("Back button clicked");
        throw new System.NotImplementedException();
    }

    private void ForwardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Logger.Info("Forward button clicked");
        throw new System.NotImplementedException();
    }

    private void DoubleForwardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Logger.Info("Forward button clicked");
        throw new System.NotImplementedException();
    }

    private void AddSongButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Logger.Info("Add song button clicked");
        var topLevel = GetTopLevel(this);

        var files = topLevel?.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false
        });

        Logger.Debug(files?.ToString() ?? string.Empty);
    }

    private void RemoveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Logger.Info("Remove button clicked");
        throw new System.NotImplementedException();
    }

    private void PauseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Logger.Info("Pause button clicked");

    }
}
