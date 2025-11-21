using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Media;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Track;

namespace Avalonix.Services.PlayableManager;

public interface IPlayableManager
{
    IMediaPlayer MediaPlayer { get; }
    IPlayable? PlayingPlayable { get; set; }
    Track? CurrentTrack { get; }
    Task<List<IPlayable>> GetPlayableItems();
    void PausePlayable();
    void ResumePlayable();
    void NextTrack();
    void TrackBefore();
    Task ChangeVolume(uint volume);
    void ResetSnuffle();
    void ResetLoop();
    void ForceStartTrackByIndex(int index);
    event Action? PlayableChanged;
    event Action<bool> PlaybackStateChanged;
    event Action TrackChanged;
    event Action<bool> ShuffleChanged;
    event Action<bool> LoopChanged;
}