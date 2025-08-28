using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.MediaPlayerFiles;
using Avalonix.Models.Media.TrackFiles;
using Microsoft.Extensions.Logging;
using NeoSimpleLogger;

namespace Avalonix.Models.Media.PlaylistFiles;

public class Playlist
{
    [JsonConstructor]
    public Playlist()
    {
    }
    public Playlist(string name, IMediaPlayer player, IDiskManager disk)
    {
        Name = name;
        _player = player;
        _disk = disk;
    }
    public void Initialize(string name, IMediaPlayer player, IDiskManager disk,ILogger logger)
    {
        Name = name;
        _player = player;
        _disk = disk;
        _logger = logger;
    }

    private IMediaPlayer _player;
    private IDiskManager _disk;
    private ILogger _logger;

    public string Name {get; set;}

    public PlaylistData PlaylistData = new();

    public async Task AddTrack(Track track)
    {
        PlaylistData.Tracks.Add(track);
        await Save();
    }

    public async Task RemoveTrack(Track track)
    {
        for (var i = 0; i < PlaylistData.Tracks.Count; i++)
        {
            if (PlaylistData.Tracks[i].TrackData.Path == track.TrackData.Path)
                PlaylistData.Tracks.Remove(PlaylistData.Tracks[i]);
            await Save();
        }
    }
    

    public async Task Save() => await _disk.SavePlaylist(this);
    
    private void UpdateLastListen() => PlaylistData.LastListen = DateTime.Now.Date;

    private void UpdateRarity() => PlaylistData.Rarity += 1;
    
    public async Task Play()
    {
        _logger.LogInformation($"Playlist {Name} has started");
        
        foreach (var track in PlaylistData.Tracks)
        {
            UpdateLastListen();
            UpdateRarity();
            
            await Save();

            _player.Play(track);

            while (!_player.IsFree)
                Task.Delay(1000).Wait();
        }

        _logger.LogInformation($"Playlist {Name} completed");
    }

    public void Pause() =>
        _player.Pause();

    public void Resume() =>
        _player.Resume();
}