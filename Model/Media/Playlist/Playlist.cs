using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.UserSettings.AvalonixSettingsFiles;
using Avalonix.Services.CacheManager;
using Avalonix.Services.DiskManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.Model.Media.Playlist;

public class Playlist : IPlayable
{
    private readonly ICacheManager _cacheManager;
    [JsonInclude] public PlaylistData Data;

    public Playlist(string name, PlaylistData playlistData, IMediaPlayer player, IDiskManager diskManager,
        ILogger logger, PlaySettings settings, ICacheManager cacheManager)
    {
        DiskManager = diskManager;
        Name = name;
        Data = playlistData;
        PlayQueue = new PlayQueue(player, logger, settings);
        _cacheManager = cacheManager;
        PlayQueue.FillQueue(Data.TracksPaths.Select(path => new Track.Track(path, _cacheManager)).ToList());
        AddObservingDirectoryFiles();
    }

    [JsonIgnore] public IDiskManager DiskManager { get; }
    [JsonInclude] public string Name { get; }
    [JsonIgnore] public PlayQueue PlayQueue { get; }

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

    public async Task SavePlaylistDataAsync()
    {
        await DiskManager.SavePlaylist(this);
    }

    private void AddObservingDirectoryFiles()
    {
        if (string.IsNullOrEmpty(Data.ObservingDirectoryPath)) return;
        var newTracksList = new List<Track.Track>();
        newTracksList.AddRange(PlayQueue.Tracks);
        newTracksList.AddRange(DiskManager.GetMusicFiles(Data.ObservingDirectoryPath)
            .Select(path => new Track.Track(path, _cacheManager)));
        PlayQueue.FillQueue(newTracksList);
    }
}