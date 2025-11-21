using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Models.Disk.DiskManager;
using Avalonix.Models.Media;
using Avalonix.Models.Media.Album;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Track;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.PlayableManager.AlbumManager;

public class AlbumManager(
    ILogger logger,
    IMediaPlayer player,
    ISettingsManager settingsManager,
    IDiskManager diskManager) : IAlbumManager
{
    public Action? TrackLoaded { get; set; }

    private readonly List<Track> _tracks = [];
    
    public IMediaPlayer MediaPlayer { get; }
    public IPlayable? PlayingPlayable { get; set; }

    public Track? CurrentTrack { get; }

    public void LoadTracks()
    {
        var paths = diskManager.GetMusicFilesForAlbums();

        foreach (var path in paths)
        {
            var track = new Track(path);
            track.Metadata.Init(path);
            track.Metadata.FillTrackMetaData();
            _tracks.Add(track);
            TrackLoaded?.Invoke();
        }
    }

    public List<Album> GetAlbums()
    {
        var albums = new List<Album>();

        foreach (var track in _tracks)
        {
            if (track.Metadata.Artist == null || track.Metadata.Album == null) continue;

            var album = ContainAlbum(track.Metadata.Artist, track.Metadata.Album);

            if (album != null) album.PlayQueue.Tracks.Add(track);
            else
            {
                album = new Album([track.TrackData.Path], player, logger,
                    settingsManager.Settings!.Avalonix.PlaySettings);
                albums.Add(album);
            }
        }

        return albums;

        Album? ContainAlbum(string artist, string albumName)
        {
            if (albums.Count == 0) return null;

            return albums.FirstOrDefault(album =>
                album.Metadata!.ArtistName == artist &&
                album.Metadata.AlbumName == albumName);
        }
    }

    public async void StartAlbum(Album album) =>
        await album.Play();

    public void RemoveAlbum(Album album)
    {
        // TODO: remove tracks
    }

    public Task<List<IPlayable>> GetPlayableItems()
    {
        LoadTracks();
        return Task.FromResult(GetAlbums().Cast<IPlayable>().ToList());
    }

    public void PausePlayable()
    {
        throw new NotImplementedException();
    }

    public void ResumePlayable()
    {
        throw new NotImplementedException();
    }

    public void NextTrack()
    {
        throw new NotImplementedException();
    }

    public void TrackBefore()
    {
        throw new NotImplementedException();
    }

    public Task ChangeVolume(uint volume)
    {
        throw new NotImplementedException();
    }

    public void ResetSnuffle()
    {
        throw new NotImplementedException();
    }

    public void ResetLoop()
    {
        throw new NotImplementedException();
    }

    public void ForceStartTrackByIndex(int index)
    {
        throw new NotImplementedException();
    }

    public event Action? PlayableChanged;
    public event Action<bool>? PlaybackStateChanged;
    public event Action? TrackChanged;
    public event Action<bool>? ShuffleChanged;
    public event Action<bool>? LoopChanged;
}