using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Model.Media.Album;

namespace Avalonix.Services.PlayableManager.AlbumManager;

public interface IAlbumManager : IPlayableManager
{
    public Action? TrackLoaded { get; set; }
    Task LoadTracks();
    List<Album> GetAlbums();
    void RemoveAlbum(Album album);
}
