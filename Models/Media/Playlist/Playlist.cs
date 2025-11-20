using System;
using System.Threading.Tasks;
using Avalonix.Models.Disk.DiskManager;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.UserSettings.AvalonixSettingsFiles;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Media.Playlist;

public record Playlist : IItem
{
    public string Name { get; }
    public PlaylistData PlaylistData { get; }
    private readonly IDiskManager _disk;
    private readonly ILogger _logger;
    private readonly PlaySettings _settings;
    public PlayQueue PlayQueue { get; }

    public Playlist(string name, PlaylistData playlistData, IMediaPlayer player, IDiskManager disk, ILogger logger,
        PlaySettings settings)
    {
        Name = name;
        PlaylistData = playlistData;
        _disk = disk;
        _logger = logger;
        _settings = settings;
        PlayQueue = new PlayQueue(player, logger, settings);

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
        _logger.LogDebug("Playlist saved {playlistName}", Name);
        await _disk.SavePlaylist(this);
    }
}
