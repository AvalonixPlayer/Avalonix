using System;
using System.Threading.Tasks;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Services.DiskManager;
using Avalonix.Services.UserSettings.AvalonixSettingsFiles;
using Microsoft.Extensions.Logging;

namespace Avalonix.Model.Media.Playlist;

public record Playlist : IPlayable
{
    public string Name { get; }
    public PlaylistData PlaylistData { get; }
    private readonly IDiskManager _disk;
    private readonly ILogger _logger;
    public PlayQueue PlayQueue { get; }

    public Playlist(string name, PlaylistData playlistData, IMediaPlayer player, IDiskManager disk, ILogger logger,
        PlaySettings settings)
    {
        Name = name;
        PlaylistData = playlistData;
        _disk = disk;
        _logger = logger;
        PlayQueue = new PlayQueue(player, logger, settings);

        PlayQueue.FillQueue(PlaylistData.Tracks);

        PlayQueue.QueueStopped += () => Task.Run(Save);
        PlayQueue.StartedNewTrack += () =>
        {
            PlaylistData.LastListen = DateTime.Now.Date;
            PlaylistData.Rarity++;
        };
    }

    public async Task Play() =>
        await PlayQueue.Play().ConfigureAwait(false);

    public void Pause() =>
        PlayQueue.Pause();

    public void Stop() =>
        PlayQueue.Stop();

    public void Resume() =>
        PlayQueue.Resume();

    public void NextTrack() =>
        PlayQueue.NextTrack();

    public void BackTrack() =>
        PlayQueue.BackTrack();

    public void ForceStartTrackByIndex(int index) =>
        PlayQueue.ForceStartTrackByIndex(index);

    public Task LoadTracksMetadata()
    {
        _ = Task.Run(() =>
        {
            foreach (var i in PlayQueue.Tracks)
            {
                i.Metadata.Init(i.TrackData.Path);
                i.Metadata.FillTrackMetaData();
            }
        });
        return Task.CompletedTask;
    }

    public async Task Save()
    {
        _logger.LogDebug("Playlist saved {playlistName}", Name);
        await _disk.SavePlaylist(this);
    }

    public bool QueueIsEmpty() =>
        PlayQueue.QueueIsEmpty();
}
