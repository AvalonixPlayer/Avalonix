using System.Threading.Tasks;
using Avalonix.Models.Media;

namespace Avalonix.Services.PlayableManager;

public interface IPlayablesManager
{
    Task StartPlayable(IPlayable playable);
}