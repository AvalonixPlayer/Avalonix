using System;
using System.Threading.Tasks;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Services.DiskManager;
using Avalonix.Services.UserSettings.AvalonixSettingsFiles;
using Microsoft.Extensions.Logging;

namespace Avalonix.Model.Media.Playlist;

public record Playlist : IPlayable
{
    private readonly IDiskManager _disk;
    private readonly ILogger _logger;

    public Playlist(PlaylistData playlistData, IMediaPlayer player, IDiskManager disk, ILogger logger,
        PlaySettings settings)
    {
        PlaylistData = playlistData;
        _disk = disk;
        _logger = logger;
        PlayQueue = new PlayQueue(player, logger, settings);
        Name = PlaylistData.Name;
        PlayQueue.FillQueue(PlaylistData.Tracks);

        Task.Run(LoadTracksMetadata).ConfigureAwait(false).GetAwaiter();

        PlayQueue.QueueStopped += () => Task.Run(Save);
        PlayQueue.StartedNewTrack += () =>
        {
            PlaylistData.LastListen = DateTime.Now.Date;
            PlaylistData.Rarity++;
        };
    }

    public PlaylistData PlaylistData { get; }
    public string Name { get; }
    public PlayQueue PlayQueue { get; }

    public async Task Play()
    {
        await PlayQueue.Play().ConfigureAwait(false);
    }

    public void Pause()
    {
        PlayQueue.Pause();
    }

    public void Stop()
    {
        PlayQueue.Stop();
    }

    public void Resume()
    {
        PlayQueue.Resume();
    }

    public void NextTrack()
    {
        PlayQueue.NextTrack();
    }

    public void BackTrack()
    {
        PlayQueue.BackTrack();
    }

    public void ForceStartTrackByIndex(int index)
    {
        PlayQueue.ForceStartTrackByIndex(index);
    }

    public async Task LoadTracksMetadata()
    {
        foreach (var i in PlayQueue.Tracks)
        {
            i.Metadata.Init(i.TrackData.Path);
            await Task.Run(i.Metadata.FillTrackMetaData);
        }
    }

    public bool QueueIsEmpty()
    {
        return PlayQueue.QueueIsEmpty();
    }

    public async Task Save()
    {
        _logger.LogDebug("Playlist saved {playlistName}", PlaylistData.Name);
        await _disk.SavePlaylist(this);
    }
}