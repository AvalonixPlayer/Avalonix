using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Models.Disk.DiskManager;
using Avalonix.Models.Media.Album;
using Avalonix.Models.Media.Track;
using File = TagLib.File;

namespace Avalonix.Services.AlbumManager;

public class AlbumManager(IDiskManager diskManager) : IAlbumManager
{
    public List<Album> GetArtistWithAlbumsWithTracks()
    {
        var result = new List<Album>();
        var paths = diskManager.GetMusicFilesForAlbums().ToList();
        
        //var tracks = 

        _ = Task.Run(() =>
        {
            foreach (var i in paths)
            {
                var track = new Track();
                track.Metadata.Init(i);
                track.Metadata.FillTrackMetaData();
            }
        });

        return result;
    }
}