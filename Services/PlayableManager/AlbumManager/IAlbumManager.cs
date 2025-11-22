using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Media;
using Avalonix.Models.Media.Album;

namespace Avalonix.Services.PlayableManager.AlbumManager;

public interface IAlbumManager : IPlayableManager
{
    public Action? TrackLoaded { get; set; }
    void LoadTracks();
    List<Album> GetAlbums();
    Task StartAlbum(IPlayable album);
    void RemoveAlbum(Album album);
}
