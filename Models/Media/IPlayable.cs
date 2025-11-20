using System.Threading.Tasks;

namespace Avalonix.Models.Media;

public interface IPlayable
{
    Task Play();
    void Pause();
    void Stop();
    void Resume();
    void NextTrack();
    void BackTrack();
}