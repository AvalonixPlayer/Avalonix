namespace Avalonix.Models.Media.MediaPlayer;

public interface IMediaPlayer
{
    bool IsFree { get; }
    
    void Play(Track.Track track);
    void Stop();
    void Pause();
    void Resume();
    void ChangeVolume(int volume); 
}