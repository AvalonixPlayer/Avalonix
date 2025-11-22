using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Services.Media.MediaPlayer;
using Avalonix.Services.Media.Playlist;
using Avalonix.Services.UserSettings.AvalonixSettingsFiles;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.Media.Album;

public record Album : IPlayable
{
    public string Name { get; }
    public AlbumMetadata? Metadata;
    private AlbumData? _albumData;
    public PlayQueue PlayQueue { get; }

    public Album(List<string> tracksPaths, IMediaPlayer player, ILogger logger, PlaySettings settings)
    {
        PlayQueue = new PlayQueue(player, logger, settings);

        Metadata = new AlbumMetadata(tracksPaths);
        _albumData = new AlbumData(tracksPaths);
        Name = Metadata.AlbumName;

        PlayQueue.FillQueue(_albumData.Tracks);
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