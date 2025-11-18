using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Media.Album;

namespace Avalonix.Services.AlbumManager;

public interface IAlbumManager
{
    public Action? TrackLoaded { get; set; }
    Task LoadTracks();
    List<Album> GetAlbums();
    void StartAlbum(Album album);
}