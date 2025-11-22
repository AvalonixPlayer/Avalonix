using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Services.UserSettings.AvalonixSettingsFiles;
using Microsoft.Extensions.Logging;

namespace Avalonix.Model.Media.Playlist;

public class PlayQueue(IMediaPlayer player, ILogger logger, PlaySettings settings)
{
    public int PlayingIndex { get; set; }
    public List<Track.Track> Tracks { get; private set; } = [];
    private PlaySettings Settings => settings;

    private readonly Random _random = new();
    private CancellationTokenSource? _cancellationTokenSource;
    public bool Paused => player.IsPaused;
    private bool _compleated;

    public Action? StartedNewTrack;
    public Action? QueueStopped;

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
        //await Save();
        _cancellationTokenSource?.Cancel();
        player.Stop();
        logger.LogDebug("Playlist stopped");
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
            _ = Play(PlayingIndex + 1);

        logger.LogDebug("User skipped track");
    }

    public void ForceStartTrackByIndex(int index) =>
        _ = Play(index);

    public void BackTrack() =>
        _ = PlayingIndex - 1 <= 0 ? Play() : Play(PlayingIndex - 1);

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

    public bool QueueIsEmpty() =>
        Tracks.Count == 0;
}
