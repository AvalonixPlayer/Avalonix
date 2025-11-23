using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Model.Media;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.Media.Track;

namespace Avalonix.Services.PlayableManager;

public interface IPlayableManager
{
    IMediaPlayer MediaPlayer { get; }
    IPlayable? PlayingPlayable { get; set; }
    Task<List<IPlayable>> GetPlaylists(); // Must move to IAlbumManager and IPlaylistManager and rename
    Task StartPlayable(IPlayable playlist);
    event Action? PlayableChanged;
    event Action<bool> PlaybackStateChanged;
    event Action TrackChanged;
    event Action<bool> ShuffleChanged;
    event Action<bool> LoopChanged;
}
