using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Media;

namespace Avalonix.Services.PlayableManager;

public interface IPlayableManager
{
    Task<List<IPlayable>> GetPlayableItems();
}
