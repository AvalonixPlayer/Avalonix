using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Model.Media;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.Media.PlayBox;
using Avalonix.Services.DiskManager;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.PlayableManager.PlayboxManager;

public class PlayboxManager(
    ILogger logger,
    IMediaPlayer player,
    ISettingsManager settingsManager,
    IDiskManager diskManager) : IPlayboxManager
{
    public IMediaPlayer MediaPlayer => player;
    public IPlayable? PlayingPlayable { get; set; }
    private CancellationTokenSource? _globalCancellationTokenSource;

    public Task<List<IPlayable>> GetPlayables()
    {
        var allMusicFiles = diskManager.GetMusicFiles();
        var playbox = new Playbox(allMusicFiles, MediaPlayer, logger, settingsManager.Settings!.Avalonix.PlaySettings);
        return Task.FromResult(new List<IPlayable> { playbox });
    }

    public void StartPlayable(IPlayable playBox)
    {
        try
        {
            _globalCancellationTokenSource?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            /* ignore */
        }
        finally
        {
            _globalCancellationTokenSource?.Dispose();
        }

        PlayingPlayable = playBox;

        PlayableChanged?.Invoke();

        _ = Task.Run(async () =>
        {
            try
            {
                await PlayingPlayable.Play().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                /* expected on cancel */
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Playlist play failed");
            }
        });
    }

    public event Action? PlayableChanged;
    public event Action<bool>? PlaybackStateChanged;
    public event Action? TrackChanged;
    public event Action<bool>? ShuffleChanged;
    public event Action<bool>? LoopChanged;
}