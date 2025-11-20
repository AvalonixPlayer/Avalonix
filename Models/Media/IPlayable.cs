using System.Threading.Tasks;
using Avalonix.Models.Media.Playlist;

namespace Avalonix.Models.Media;

public interface IPlayable
{
    PlayQueue PlayQueue { get; }
    Task Play();
    void Pause();
    void Stop();
    void Resume();
    void NextTrack();
    void BackTrack();
    void ForceStartTrackByIndex(int index);
    Task LoadTracksMetadata();
    bool QueueIsEmpty();
}