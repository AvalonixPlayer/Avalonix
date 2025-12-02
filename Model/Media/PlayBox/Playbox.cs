using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.Media.Playlist;
using Avalonix.Model.UserSettings.AvalonixSettingsFiles;
using Microsoft.Extensions.Logging;

namespace Avalonix.Model.Media.PlayBox;

public class Playbox : IPlayable
{
    public Playbox(List<string> tracksPaths, IMediaPlayer player, ILogger logger, PlaySettings settings)
    {
        PlayQueue = new PlayQueue(player, logger, settings);
        PlayQueue.FillQueue(tracksPaths.Select(path => new Track.Track(path)).ToList());
        Task.Run(LoadBasicTracksMetadata).GetAwaiter();
    }

    public string Name => "PlayBox";
    public PlayQueue PlayQueue { get; }

    public async Task Play()
    {
        await PlayQueue.Play();
    }

    public void Pause()
    {
        PlayQueue.Pause();
    }

    public void Stop()
    {
        PlayQueue.Stop();
    }

    public void Resume()
    {
        PlayQueue.Resume();
    }

    public void NextTrack()
    {
        PlayQueue.NextTrack();
    }

    public void BackTrack()
    {
        PlayQueue.BackTrack();
    }

    public void ForceStartTrackByIndex(int index)
    {
        PlayQueue.ForceStartTrackByIndex(index);
    }

    public async Task LoadBasicTracksMetadata()
    {
        foreach (var track in PlayQueue.Tracks)
        {
            await Task.Run(() => track.Metadata.FillBasicTrackMetaData(track.TrackData.Path));
        }
    }

    public bool QueueIsEmpty()
    {
        return PlayQueue.QueueIsEmpty();
    }
}