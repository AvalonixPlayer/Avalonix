using System.Collections.Generic;
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
        
        List<Track.Track> tracks = [];
        foreach (var trackPath in tracksPaths)
        {
            var track = new Model.Media.Track.Track(trackPath);
            track.Metadata.Init(trackPath);
            track.Metadata.FillTrackMetaData();
            tracks.Add(track);
        }
        
        PlayQueue.FillQueue(tracks);
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

    public Task LoadTracksMetadata()
    {
        _ = Task.Run(() =>
        {
            foreach (var i in PlayQueue.Tracks)
            {
                i.Metadata.Init(i.TrackData.Path);
                i.Metadata.FillTrackMetaData();
            }
        });
        return Task.CompletedTask;
    }

    public bool QueueIsEmpty() =>
        PlayQueue.QueueIsEmpty();
}