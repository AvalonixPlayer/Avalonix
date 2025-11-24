using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.Media.Playlist;
using Avalonix.Services.UserSettings.AvalonixSettingsFiles;
using Microsoft.Extensions.Logging;

namespace Avalonix.Model.Media.Album;

public record Album : IPlayable
{
    public string Name { get; }
    public AlbumMetadata? Metadata;
    public PlayQueue PlayQueue { get; }

    public Album(List<string> tracksPaths, IMediaPlayer player, ILogger logger, PlaySettings settings)
    {
        PlayQueue = new PlayQueue(player, logger, settings);

        Metadata = new AlbumMetadata(tracksPaths);
        Name = Metadata.AlbumName;

        PlayQueue.FillQueue(tracksPaths.Select(path => new Track.Track(path)).ToList());
        Task.Run(LoadTracksMetadata);
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
