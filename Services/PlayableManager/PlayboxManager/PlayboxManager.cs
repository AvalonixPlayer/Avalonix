using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Model.Media;
using Avalonix.Model.Media.MediaPlayer;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.PlayableManager.PlayboxManager;

public class PlayboxManager(
    ILogger logger,
    IMediaPlayer player) : IPlayboxManager
{
    public IMediaPlayer MediaPlayer => player;
    public IPlayable? PlayingPlayable { get; set; }
    private CancellationTokenSource? _globalCancellationTokenSource;
    
    public Task<List<IPlayable>> GetPlayables()
    {
        throw new NotImplementedException();
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