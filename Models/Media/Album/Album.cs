using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Playlist;
using Avalonix.Models.UserSettings;
using Avalonix.Models.UserSettings.AvalonixSettingsFiles;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Media.Album;

public record Album
{
    public AlbumMetadata? Metadata { get; private set; }
    public AlbumData? AlbumData { get; private set; }
    private IMediaPlayer Player { get; }
    private ILogger Logger { get; }
    private PlaySettings Settings { get; }
    public PlayQueue PlayQueue { get; }

    private readonly Random _random = new();
    private CancellationTokenSource? _cancellationTokenSource;
    public bool Paused => Player.IsPaused;
    private bool _compleated;

    public Album(List<string> tracksPaths, IMediaPlayer player, ILogger logger, PlaySettings settings, PlayQueue playQueue)
    {
        Player = player;
        Logger = logger;
        Settings = settings;
        PlayQueue = playQueue;

        AlbumData = new AlbumData(tracksPaths);
        Metadata = new AlbumMetadata(tracksPaths);
    }

    public async Task Play(int startSong = 0)
    {
        while (true)
        {
            _cancellationTokenSource?.CancelAsync();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            Logger.LogDebug("Album {Name} has started", Metadata!.AlbumName);

            while (true)
            {
                for (var i = startSong; i < PlayQueue.Tracks.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    PlayQueue.PlayingIndex = i;
                    var track = PlayQueue.Tracks[PlayQueue.PlayingIndex];

                    Player.Play(track);

                    while (!Player.IsFree && !cancellationToken.IsCancellationRequested)
                        await Task.Delay(1000, cancellationToken);

                    if (cancellationToken.IsCancellationRequested) break;
                }

                if (Settings.Loop)
                {
                    startSong = 0;
                    continue;
                }

                Logger.LogDebug("Album {Name} completed", Metadata.AlbumName);
                _compleated = true;
                break;
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                PlayQueue.FillQueue(AlbumData!.Tracks);
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
        _cancellationTokenSource?.Cancel();
        Player.Stop();
        Logger.LogDebug("Album stopped");
    }

    public void NextTrack()
    {
        if (_compleated)
        {
            PlayQueue.FillQueue(AlbumData!.Tracks);
            _ = Play();
            _compleated = false;
            return;
        }

        if (PlayQueue.PlayingIndex + 1 >= PlayQueue.Tracks.Count)
        {
            PlayQueue.FillQueue(AlbumData!.Tracks);
            _ = Play();
        }
        else
            _ = Play(PlayQueue.PlayingIndex + 1);
        Logger.LogDebug("User skipped track");
    }

    public void ForceStartTrackByIndex(int index) =>
        _ = Play(index);

    public void BackTrack() =>
        _ = PlayQueue.PlayingIndex - 1 <= 0 ? Play() : Play(PlayQueue.PlayingIndex - 1);

    public void Pause()
    {
        Player.Pause();
        Logger.LogDebug("Album paused");
    }

    public void Resume()
    {
        Player.Resume();
        Logger.LogDebug("Album resumed");
    }
}