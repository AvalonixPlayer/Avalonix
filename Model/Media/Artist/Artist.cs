using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.UserSettings.AvalonixSettingsFiles;
using Microsoft.Extensions.Logging;

namespace Avalonix.Model.Media.Artist;

public record Artist : IPlayable
{
    public string Name { get; }
    public PlayQueue PlayQueue { get; }

    public Artist(List<Track.Track> tracks, IMediaPlayer player, ILogger logger, PlaySettings settings)
    {
        PlayQueue = new PlayQueue(player, logger, settings);
        Name = tracks[0].Metadata.Artist ?? string.Empty;
        PlayQueue.FillQueue(tracks);
    }

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

    public void ForceStartTrack(Track.Track track)
    {
        PlayQueue.ForceStartTrack(track);
    }

    public async Task LoadTracksMetadata()
    {
        foreach (var track in PlayQueue.Tracks) await track.FillPrimaryMetaData();
    }

    public bool QueueIsEmpty()
    {
        return PlayQueue.QueueIsEmpty();
    }
}