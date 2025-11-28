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

    public Playlist(string title, PlaylistData playlistData, IMediaPlayer player, IDiskManager disk, ILogger logger,
        PlaySettings settings)
    {
        PlaylistData = playlistData;
        _disk = disk;
        _logger = logger;
        PlayQueue = new PlayQueue(player, logger, settings);
        PlaylistData.Name = title;
        Name = playlistData.Name;
        PlayQueue.FillQueue(PlaylistData.Tracks);

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
        await Task.Run(LoadBasicTracksMetadata);
        await PlayQueue.Play();
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

    public async Task LoadBasicTracksMetadata()
    {
        foreach (var i in PlayQueue.Tracks)
        {
            await Task.Run(() => i.Metadata.FillBasicTrackMetaData(i.TrackData.Path));
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