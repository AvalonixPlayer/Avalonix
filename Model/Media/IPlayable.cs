using System.Threading.Tasks;

namespace Avalonix.Model.Media;

public interface IPlayable
{
    string Name { get; }
    PlayQueue PlayQueue { get; }
    Task Play();
    void Pause();
    void Stop();
    void Resume();
    void NextTrack();
    void BackTrack();
    void ForceStartTrackByIndex(int index);
    Task LoadBasicTracksMetadata();
    bool QueueIsEmpty();
}