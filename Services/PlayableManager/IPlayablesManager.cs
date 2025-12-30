using System;
using System.Threading.Tasks;
using Avalonix.Model.Media;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.Media.Track;

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
    void ForceStartTrack(Track track);

    event Action? PlayableChanged;
    event Action<bool> PlaybackStateChanged;
    event Action TrackChanged;
    event Action<bool> ShuffleChanged;
    event Action<bool> LoopChanged;
}