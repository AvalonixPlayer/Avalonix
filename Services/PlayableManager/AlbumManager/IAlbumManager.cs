using System;
using System.Collections.Generic;
using Avalonix.Model.Media.Album;

namespace Avalonix.Services.PlayableManager.AlbumManager;

public interface IAlbumManager : IPlayableManager
{
    public Action? TrackLoaded { get; set; }
    void LoadTracks();
    List<Album> GetAlbums();
    void RemoveAlbum(Album album);
}
