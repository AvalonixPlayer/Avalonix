namespace Avalonix.Models.Media;

public interface IMediaPlayer
{
    bool IsFree { get; }
    
    void Play(Track track);
    void Stop();
    void Pause();
    void Resume();
    void ChangeVolume(int volume); 
}