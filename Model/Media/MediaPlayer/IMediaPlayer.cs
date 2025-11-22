using System;
using System.Threading.Tasks;

namespace Avalonix.Services.Media.MediaPlayer;

public interface IMediaPlayer
{
    bool IsFree { get; }
    bool IsPaused {get; }
    public Track.Track? CurrentTrack { get; }
    
    void Play(Track.Track track);
    void Stop();
    void Pause();
    void Resume();
    Task ChangeVolume(uint volume);
    double GetPosition();
    void SetPosition(double position);
    event Action<bool> PlaybackStateChanged;
    event Action TrackChanged;
}