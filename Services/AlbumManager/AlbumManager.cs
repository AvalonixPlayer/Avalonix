using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Models.Disk.DiskManager;
using Avalonix.Models.Media.Album;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Track;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;
using File = TagLib.File;

namespace Avalonix.Services.AlbumManager;

public class AlbumManager(
    IMediaPlayer player,
    ILogger logger,
    ISettingsManager settingsManager,
    IDiskManager diskManager) : IAlbumManager
{
    public List<Album> GetAlbums()
    {
        var paths = diskManager.GetMusicFilesForAlbums().ToList();

        var tracks = new List<Track>();

        _ = Task.Run(() =>
        {
            foreach (var path in paths)
            {
                var track = new Track();
                track.Metadata.Init(path);
                track.Metadata.FillTrackMetaData();
                tracks.Add(track);
            }
        });

        var albums = new List<Album>();

        foreach (var track in tracks)
        {
            if (track.Metadata.Artist == null || track.Metadata.Album == null) continue;

            Album? album;
            if (ContaintAlbum(track.Metadata.Artist, track.Metadata.Album) != null)
            {
                album = ContaintAlbum(track.Metadata.Artist, track.Metadata.Album);
                album!.PlayQueue.Tracks.Add(track);
            }
            else
            {
                album = new Album([track.TrackData.Path], player, logger,
                    settingsManager.Settings!.Avalonix.PlaySettings);
                albums.Add(album);
            }
        }

        return albums;

        Album? ContaintAlbum(string artist, string albumName) =>
            albums.First(album =>
                album.Metadata!.ArtistName == artist && album.Metadata.AlbumName == albumName);
    }
}