using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Model.Media;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.Media.PlayBox;
using Avalonix.Services.CacheManager;
using Avalonix.Services.DiskManager;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.PlayableManager.PlayboxManager;

public class PlayboxManager(
    ILogger logger,
    IMediaPlayer player,
    ISettingsManager settingsManager,
    IDiskManager diskManager,
    ICacheManager cacheManager) : IPlayboxManager
{
    public IMediaPlayer MediaPlayer => player;
    public IPlayable? PlayingPlayable { get; set; }
    public CancellationTokenSource GlobalCancellationTokenSource { get; }


    public void StartPlayable(IPlayable playBox)
    {
        try
        {
            GlobalCancellationTokenSource?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            GlobalCancellationTokenSource?.Dispose();
        }

        PlayingPlayable = playBox;

        PlayableChanged?.Invoke();

        _ = Task.Run(async () =>
        {
            try
            {
                await PlayingPlayable.Play();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine(PlayingPlayable.PlayQueue.Tracks);
                logger.LogError(ex.Message, "Playbox play failed");
            }
        });
    }

    public Task<List<IPlayable>> GetPlayables()
    {
        var settings = settingsManager.Settings.Avalonix;
        var allMusicFiles = diskManager.GetMusicFiles();
        var playbox = new Playbox(allMusicFiles, MediaPlayer, logger, settings.PlaySettings, cacheManager);
        return Task.FromResult(new List<IPlayable> { playbox });
    }

    public event Action? PlayableChanged;
    public event Action<bool>? PlaybackStateChanged;
    public event Action? TrackChanged;
    public event Action<bool>? ShuffleChanged;
    public event Action<bool>? LoopChanged;
}