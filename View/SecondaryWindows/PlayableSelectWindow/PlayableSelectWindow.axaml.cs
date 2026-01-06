using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonix.Model.Media;
using Avalonix.Services.WindowManager;
using Avalonix.ViewModel.PlayableSelectViewModel;
using Microsoft.Extensions.Logging;

namespace Avalonix.View.SecondaryWindows.PlayableSelectWindow;

public partial class PlayableSelectWindow : Window, ISecondaryWindow
{
    private readonly ILogger<WindowManager> _logger;
    private readonly List<IPlayable> _playables;
    private readonly IPlayableSelectViewModel _vm;

    public PlayableSelectWindow(ILogger<WindowManager> logger, IPlayableSelectViewModel vm)
    {
        InitializeComponent();
        _logger = logger;
        _vm = vm;
        _logger.LogInformation("PlayableCreateWindow opened");
        _playables = Task.Run(async () => await _vm.GetPlayableItems()).Result;
        InitializeControls();
    }
    
    private void SearchBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = SearchBox.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            PlaylistBox.ItemsSource = _playables.Select(p => p.Name);
            return;
        }

        var stringPlaylists = _vm.SearchItem(text, _playables).Select(p => p.Name).ToList();
        PlaylistBox.ItemsSource = stringPlaylists;
    }

    private async void ActionSelectedPlayable(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            var castedSender = (ListBox)sender!;
            _logger.LogInformation(castedSender.SelectedItem?.ToString());

            var searchBoxText = SearchBox.Text;
            var playablesResult = string.IsNullOrWhiteSpace(searchBoxText)
                ? _playables
                : _playables.Where(i => i.Name.Contains(searchBoxText, StringComparison.CurrentCultureIgnoreCase))
                    .ToList();

            var selectedPlayable =
                playablesResult
                    [castedSender.SelectedIndex];
            await _vm.ExecuteAction(selectedPlayable);
            Close();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while starting playlist in SelectWindow: {ex}", ex.Message);
        }
    }

    public void InitializeControls()
    {
        PlaylistBox.ItemsSource = _playables.Select(p => p.Name).ToList();
        Title = _vm.Strategy.WindowTitle;
        SearchBox.Watermark = _vm.Strategy.ActionButtonText;
    }
}