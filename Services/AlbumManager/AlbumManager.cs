using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonix.Models.Disk.DiskManager;
using Avalonix.Models.Media.Album;
using Avalonix.Models.Media.Track;
using File = TagLib.File;

namespace Avalonix.Services.AlbumManager;

public class AlbumManager(IDiskManager diskManager) : IAlbumManager
{
    public Dictionary<List<string>,Track> GetArtistWithAlbumsWithTracks()
    {
        var result = new Dictionary<List<string>,Track>();
        var paths = diskManager.GetMusicFilesForAlbums().ToList();

        new Thread(() =>
        {
            foreach (var i in paths)
            {
                var track = new Track();
                track.Metadata.Init(i);
                track.Metadata.FillTrackMetaData();
            }
        }).Start();

        return result;
    }
}