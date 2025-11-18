using System;
using System.Collections.Generic;
using System.Linq;
using Avalonix.Models.Disk.DiskManager;
using File = TagLib.File;

namespace Avalonix.Services.AlbumManager;

public class AlbumManager(IDiskManager diskManager) : IAlbumManager
{
    public Dictionary<ArtistWithAlbum, List<string>> GetArtistWithAlbumsWithPathToTracks()
    {
        var result = new Dictionary<ArtistWithAlbum, List<string>>();
        var paths = diskManager.GetMusicFilesForAlbums().ToList();

        var albumsWithArtists = paths.Select(GetArtistWithAlbum).ToList();
        
        foreach (var al in albumsWithArtists)
        {
            if (result.ContainsKey(al))
            {
                var index = result.Keys.ToList().IndexOf(al);
                result.ElementAt(index).Value = al
            }
            else
            {
                result.Add(al, []);
            }
                
        }

        return result;

        ArtistWithAlbum GetArtistWithAlbum(string path)
        {
            var track = File.Create(path);
            if (string.IsNullOrEmpty(track.Tag?.Album) || string.IsNullOrEmpty(track.Tag?.FirstAlbumArtist))
                return null!;
            return new ArtistWithAlbum(track.Tag.FirstAlbumArtist, track.Tag.Album);
        }
    }
}

public record ArtistWithAlbum(string Name, string Album)
{
    public string Name = Name;
    public string Album = Album;
}