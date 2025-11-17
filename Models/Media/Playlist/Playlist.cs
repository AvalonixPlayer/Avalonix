using System;
using System.Threading.Tasks;
using Avalonix.Models.Disk.DiskManager;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.UserSettings.AvalonixSettingsFiles;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Media.Playlist;

public record Playlist
{
    public string Name { get; }
    public PlaylistData PlaylistData { get; }
    private IDiskManager Disk { get; }
    private ILogger Logger { get; }
    private PlaySettings Settings { get; }
    public PlayQueue PlayQueue { get; }

    public Playlist(string name, PlaylistData playlistData, IMediaPlayer player, IDiskManager disk, ILogger logger,
        PlaySettings settings)
    {
        Name = name;
        PlaylistData = playlistData;
        Disk = disk;
        Logger = logger;
        Settings = settings;
        PlayQueue = new PlayQueue(player, logger, Settings);

        PlayQueue.FillQueue(PlaylistData.Tracks);

        PlayQueue.QueueStopped += () => Task.Run(Save);
        PlayQueue.StartedNewTrack += () =>
        {
            PlaylistData.LastListen = DateTime.Now.Date;
            PlaylistData.Rarity++;
        };
    }
    
    public async Task Save()
    {
        Logger.LogDebug("Playlist saved {playlistName}", Name);
        await Disk.SavePlaylist(this);
    }
}