using Avalonix.Model.Media.Album;

namespace Avalonix.Services.PlayableManager.AlbumManager;

public interface IAlbumManager : IPlayableManager
{
    void RemoveAlbum(Album album);
}