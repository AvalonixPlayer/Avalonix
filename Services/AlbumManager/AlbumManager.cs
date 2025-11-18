using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Models.Disk.DiskManager;
using Avalonix.Models.Media.Album;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Track;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.AlbumManager;

public class AlbumManager(
    ILogger logger,
    IMediaPlayer player,
    ISettingsManager settingsManager,
    IDiskManager diskManager) : IAlbumManager
{
    public Action? TrackLoaded { get; set; }
    
    private readonly List<Track> _tracks = [];

    public Task LoadTracks()
    {
        _ = Task.Run(() =>
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
        });
        return Task.CompletedTask;
    }
    
    public List<Album> GetAlbums()
    {
        var albums = new List<Album>();

        foreach (var track in _tracks)
        {
            if (track.Metadata.Artist == null || track.Metadata.Album == null) continue;

            var album = ContaintAlbum(track.Metadata.Artist, track.Metadata.Album);

            if (album != null) album.PlayQueue.Tracks.Add(track);
            else
            {
                album = new Album([track.TrackData.Path], player, logger,
                    settingsManager.Settings!.Avalonix.PlaySettings);
                albums.Add(album);
            }
        }

        return albums;

        Album? ContaintAlbum(string artist, string albumName)
        {
            if (albums.Count == 0) return null;

            return albums.FirstOrDefault(album =>
                album.Metadata!.ArtistName == artist &&
                album.Metadata.AlbumName == albumName);
        }
    }
}