using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Services.UserSettings.AvalonixSettingsFiles;
using Microsoft.Extensions.Logging;

namespace Avalonix.Model.Media;

public class PlayQueue(IMediaPlayer player, ILogger logger, PlaySettings settings)
{
    private readonly Random _random = new();
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _compleated;
    public Action? QueueStopped;

    public Action? StartedNewTrack;
    public int PlayingIndex { get; set; }
    public List<Track.Track> Tracks { get; private set; } = [];
    private PlaySettings Settings => settings;
    public bool Paused => player.IsPaused;

    public void FillQueue(List<Track.Track> tracks)
    {
        PlayingIndex = 0;
        Tracks = tracks;

        if (Settings.Shuffle)
            Tracks = Tracks.OrderBy(_ => _random.Next()).ToList();
    }

    public async Task Play(int startSong = 0)
    {
        while (true)
        {
            _cancellationTokenSource?.CancelAsync();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            while (true)
            {
                for (var i = startSong; i < Tracks.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    PlayingIndex = i;
                    var track = Tracks[PlayingIndex];
                    await Task.Run(() => track.FillTrackMetaData());

                    StartedNewTrack?.Invoke();
                    track.IncreaseRarity(1);

                    track.UpdateLastListenDate();

                    player.Play(track);

                    while (!player.IsFree && !cancellationToken.IsCancellationRequested)
                        await Task.Delay(1000, cancellationToken);

                    if (cancellationToken.IsCancellationRequested) break;
                }

                if (Settings.Loop)
                {
                    startSong = 0;
                    continue;
                }

                _compleated = true;
                break;
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                FillQueue(Tracks);
                if (Settings.Loop)
                {
                    startSong = 0;
                    continue;
                }
            }

            break;
        }
    }

    public void Stop()
    {
        QueueStopped?.Invoke();
        _cancellationTokenSource?.Cancel();
        player.Stop();
        logger.LogDebug("Playable stopped");
    }

    public void NextTrack()
    {
        if (_compleated)
        {
            FillQueue(Tracks);
            _ = Play();
            _compleated = false;
            return;
        }

        if (PlayingIndex + 1 >= Tracks.Count)
        {
            FillQueue(Tracks);
            _ = Play();
        }
        else
        {
            _ = Play(PlayingIndex + 1);
        }

        logger.LogDebug("User skipped track");
    }

    public void ForceStartTrackByIndex(int index)
    {
        _ = Play(index);
    }

    public void BackTrack()
    {
        _ = PlayingIndex - 1 <= 0 ? Play() : Play(PlayingIndex - 1);
    }

    public void Pause()
    {
        player.Pause();
        logger.LogDebug("Playlist paused");
    }

    public void Resume()
    {
        player.Resume();
        logger.LogDebug("Playlist resumed");
    }

    public bool QueueIsEmpty()
    {
        return Tracks.Count == 0;
    }
}