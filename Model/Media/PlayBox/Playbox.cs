using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.UserSettings.AvalonixSettingsFiles;
using Avalonix.Services.CacheManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.Model.Media.PlayBox;

public class Playbox : IPlayable
{
    private readonly ICacheManager _cacheManager;

    public Playbox(List<string> tracksPaths, IMediaPlayer player, ILogger logger, PlaySettings settings,
        ICacheManager cacheManager)
    {
        PlayQueue = new PlayQueue(player, logger, settings);
        _cacheManager = cacheManager;
        PlayQueue.FillQueue(tracksPaths.Select(path => new Track.Track(path, _cacheManager)).ToList());
        Task.Run(LoadTracksMetadata).GetAwaiter();
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

    public async Task LoadTracksMetadata()
    {
        foreach (var track in PlayQueue.Tracks) await track.FillPrimaryMetaData();
    }

    public bool QueueIsEmpty()
    {
        return PlayQueue.QueueIsEmpty();
    }
}