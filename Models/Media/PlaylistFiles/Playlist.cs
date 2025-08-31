using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.MediaPlayerFiles;
using Avalonix.Models.Media.TrackFiles;
using Avalonix.Models.UserSettings;
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

    public async Task Initialize(string name, IMediaPlayer player, IDiskManager disk, ILogger logger)
    {
        Name = name;
        _player = player;
        _disk = disk;
        _logger = logger;
        _settings = await _disk.GetSettings();
    }

    private IMediaPlayer _player;
    private IDiskManager _disk;
    private ILogger _logger;
    private Settings _settings;
    public string Name { get; private set; }

    public PlaylistData PlaylistData = new();

    public async Task AddTrack(Track track)
    {
        PlaylistData.Tracks.Add(track);
        await Save();
    }

    public async Task RemoveTrack(Track track)
    {
        for (var i = 0; i < PlaylistData.Tracks.Count; i++)
            if (PlaylistData.Tracks[i].TrackData.Path == track.TrackData.Path)
                PlaylistData.Tracks.Remove(PlaylistData.Tracks[i]);
        await Save();
    }


    public async Task Save() => await _disk.SavePlaylist(this);

    private void UpdateLastListen() => PlaylistData.LastListen = DateTime.Now.Date;

    private void UpdateRarity() => PlaylistData.Rarity++;

    public async Task Play()
    {
        var random = new Random();
        var tracks = PlaylistData.Tracks;
        if(_settings.Avalonix.Playlists.Shuffle)
            tracks = tracks.OrderBy(_ => random.Next()).ToList();

        _logger.LogInformation("Playlist {Name} has started", Name);

        foreach (var track in tracks)
        {
            UpdateLastListen();
            UpdateRarity();

            await Save();

            _player.Play(track);
            
            while (!_player.IsFree)
                await Task.Delay(1000);
        }

        if (_settings.Avalonix.Playlists.Loop) await Play();

        _logger.LogInformation("Playlist {Name} completed", Name);
    }

    public void Stop() => 
        _player.Stop();

    public void Pause() =>
        _player.Pause();

    public void Resume() =>
        _player.Resume();

}