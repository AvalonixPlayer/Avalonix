using Avalonix.Models.Media.TrackFiles;

namespace Avalonix.Models.Media.MediaPlayerFiles;

public interface IMediaPlayer
{
    bool IsFree { get; }
    
    void Play(Track track);
    void Stop();
    void Pause();
    void Resume();
    void ChangeVolume(int volume); 
}