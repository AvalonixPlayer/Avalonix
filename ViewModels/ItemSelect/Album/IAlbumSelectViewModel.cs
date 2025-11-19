using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.ViewModels.Strategy;

namespace Avalonix.ViewModels.ItemSelect.Album;

public interface IAlbumSelectViewModel : IItemSelectViewModel
{
    Task<List<Models.Media.Album.Album>> GetItem();
    List<Models.Media.Album.Album> SearchItem(string text, List<Models.Media.Album.Album> albums);
    Task ExecuteAction(Models.Media.Album.Album album);
    IAlbumWindowStrategy Strategy { get; }
}
