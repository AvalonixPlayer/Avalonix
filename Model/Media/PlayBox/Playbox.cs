using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Model.Media.Album;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.Media.Playlist;
using Avalonix.Services.UserSettings.AvalonixSettingsFiles;
using Microsoft.Extensions.Logging;
using TagLib.Flac;

namespace Avalonix.Model.Media.PlayBox;

public class Playbox : IPlayable
{
    public string Name => "PlayBox";
    public PlayQueue PlayQueue { get; }

    public Playbox(List<string> tracksPaths, IMediaPlayer player, ILogger logger, PlaySettings settings)
    {
        PlayQueue = new PlayQueue(player, logger, settings);
        PlayQueue.FillQueue(tracksPaths.Select(path => new Track.Track(path)).ToList());
        Task.Run(LoadTracksMetadata).ConfigureAwait(false).GetAwaiter();
    }

    public async Task Play() =>
        await PlayQueue.Play().ConfigureAwait(false);

    public void Pause() =>
        PlayQueue.Pause();

    public void Stop() =>
        PlayQueue.Stop();

    public void Resume() =>
        PlayQueue.Resume();

    public void NextTrack() =>
        PlayQueue.NextTrack();

    public void BackTrack() =>
        PlayQueue.BackTrack();

    public void ForceStartTrackByIndex(int index) =>
        PlayQueue.ForceStartTrackByIndex(index);

    public async Task LoadTracksMetadata()
    {
        foreach (var i in PlayQueue.Tracks)
        {
            i.Metadata.Init(i.TrackData.Path);
            await Task.Run(i.Metadata.FillTrackMetaData);
        }
    }

    public bool QueueIsEmpty() =>
        PlayQueue.QueueIsEmpty();
}