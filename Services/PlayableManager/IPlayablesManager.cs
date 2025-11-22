using System;
using System.Threading.Tasks;
using Avalonix.Models.Media;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Track;

namespace Avalonix.Services.PlayableManager;

public interface IPlayablesManager
{
    IMediaPlayer MediaPlayer { get; }
    IPlayable? PlayingPlayable { get; }
    Track? CurrentTrack { get; }
    Task StartPlayable(IPlayable playable);
    Task ChangeVolume(uint volume);
    
    void PausePlayable();
    void ResumePlayable();
    void NextTrack();
    void TrackBefore();
    void ResetSnuffle();
    void ResetLoop();
    void ForceStartTrackByIndex(int index);

    event Action? PlayableChanged;
    event Action<bool> PlaybackStateChanged;
    event Action TrackChanged;
    event Action<bool> ShuffleChanged;
    event Action<bool> LoopChanged;
}