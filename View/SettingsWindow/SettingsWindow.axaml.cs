using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.UserSettings;
using Avalonix.Services.SettingsManager;
using Avalonix.ViewModel.Settings;
using Microsoft.Extensions.Logging;

namespace Avalonix.View.SettingsWindow;

public partial class SettingsWindow : Window
{
    private readonly ILogger _logger;
    private readonly Settings _settings;
    private readonly ISettingsManager _settingsManager;
    private readonly ISettingsWindowViewModel _vm;
    private string? _autoCoverPath;
    private IMediaPlayer _mediaPlayer;

    public SettingsWindow(ISettingsWindowViewModel vm, ISettingsManager settingsManager, ILogger logger,
        IMediaPlayer mediaPlayer)
    {
        _vm = vm;
        _logger = logger;
        _settingsManager = settingsManager;
        _mediaPlayer = mediaPlayer;
        InitializeComponent();
        _logger.LogInformation("Settings Window Open");

        _settings = _settingsManager.Settings!;
        _autoCoverPath = _settings.Avalonix.AutoAlbumCoverPath;

        LoadMusicPaths();
        LoadAutoCover();
    }

    protected override void OnClosed(EventArgs e)
    {
        _logger.LogInformation("Settings window is closed");
        base.OnClosed(e);
    }

    private async void ApplySettingsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _settings.Avalonix.MusicFilesPaths = PathsBox.Items.ToList().Select(x => x?.ToString()).ToList();
        _settings.Avalonix.AutoAlbumCoverPath = _autoCoverPath;
        await _settingsManager.SaveSettingsAsync();
    }

    private void ExitButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }


    private async void AddImage_OnClick(object? sender, RoutedEventArgs e)
    {
        _autoCoverPath = await _vm.OpenCoverFileDialogAsync(this) ?? string.Empty;
        LoadAutoCover();
    }

    private void LoadMusicPaths()
    {
        if (_settings.Avalonix.MusicFilesPaths.Count <= 0) return;
        foreach (var musicFilePath in _settings.Avalonix.MusicFilesPaths)
            PathsBox.Items.Add(musicFilePath);
    }
    
    private void AddPath_OnClick(object? sender, RoutedEventArgs e)
    {
        if(Directory.Exists(PathToAdd.Text) && !PathsBox.Items.Contains(PathToAdd.Text))
            PathsBox.Items.Add(PathToAdd.Text);
    }
    
    private void RemoveSelectedPath_OnClick(object? sender, RoutedEventArgs e)
    {
        PathsBox.Items.Remove(PathsBox.SelectedItem);
    }

    private void LoadAutoCover()
    {
        if (_autoCoverPath == null) return;
        _ = Task.Run(() =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                var coverBites = File.ReadAllBytes(_autoCoverPath);
                using var memoryStream = new MemoryStream(coverBites);
                AlbumCover.Child = new Image
                {
                    Source = new Bitmap(memoryStream),
                    Stretch = Stretch.UniformToFill
                };
            });
        });
    }

    private void EqualizerFx1_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        _settings.Avalonix.EqualizerSettings._fxs[0] = (float)e.NewValue;
        _mediaPlayer.SetParametersEQ(0, 100, (float)e.NewValue);
    }

    private void EqualizerFx2_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        _settings.Avalonix.EqualizerSettings._fxs[1] = (float)e.NewValue;
        _mediaPlayer.SetParametersEQ(1, 1000, (float)e.NewValue);
    }

    private void EqualizerFx3_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        _settings.Avalonix.EqualizerSettings._fxs[2] = (float)e.NewValue;
        _mediaPlayer.SetParametersEQ(2, 8000, (float)e.NewValue);
    }

    private void EqualizersReset_OnClick(object? sender, RoutedEventArgs e)
    {
        for (int i = 0; i < 2; i++)
        {
            _settings.Avalonix.EqualizerSettings._fxs[i] = 0;
        }

        _mediaPlayer.SetParametersEQ(0, 100, 0);
        _mediaPlayer.SetParametersEQ(1, 1000, 0);
        _mediaPlayer.SetParametersEQ(2, 8000, 0);

        Equalizer1.Value = 0;
        Equalizer2.Value = 0;
        Equalizer3.Value = 0;
    }
}