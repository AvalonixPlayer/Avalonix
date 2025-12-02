using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.UserSettings.AvalonixSettingsFiles;
using Avalonix.Services.DiskManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.Model.Media.Playlist;

public class Playlist : IPlayable
{
    [JsonInclude] public string Name { get; }
    [JsonInclude] public PlaylistData Data;
    [JsonIgnore] public PlayQueue PlayQueue { get; }
    [JsonIgnore] public IDiskManager DiskManager { get; }

    public Playlist(string name, PlaylistData playlistData, IMediaPlayer player, IDiskManager diskManager,
        ILogger logger, PlaySettings settings)
    {
        DiskManager = diskManager;
        Name = name;
        Data = playlistData;
        PlayQueue = new PlayQueue(player, logger, settings);
        PlayQueue.FillQueue(Data.TracksPaths.Select(path => new Track.Track(path)).ToList());
        AddObservingDirectoryFiles();
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
            .Select(path => new Track.Track(path)));
        PlayQueue.FillQueue(newTracksList);
    }
}