using System.Threading.Tasks;

namespace Avalonix.ViewModels.ItemSelect;

public interface IItemSelectViewModel
{
    public Task<T> GetItems<T>();
    public IAlbum

}
