using System.Threading.Tasks;

namespace Avalonix.Services.PlayableManager;

public interface IPlayablesManager
{
    void StartPlayable(IPlayableManager playableManager);
}