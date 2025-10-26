using System.Threading.Tasks;

namespace Avalonix.Models.Media.MediaPlayer;

public interface IMediaPlayer
{
    bool IsFree { get; }
    bool IsPaused {get; }
    public Track.Track? CurrentTrack { get; }
    
    void Play(Track.Track track);
    void Stop();
    void Pause();
    void Resume();
    Task ChangeVolume(int volume); 
}