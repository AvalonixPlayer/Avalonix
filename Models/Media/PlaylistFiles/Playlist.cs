using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.MediaPlayerFiles;
using Avalonix.Models.Media.TrackFiles;
using Avalonix.Models.UserSettings;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Media.PlaylistFiles;

public class Playlist : ILoadWithDependency
{
    [JsonIgnore]
    public IMediaPlayer Player { get; set; } = null!;
    [JsonIgnore]
    public IDiskManager Disk { get; set; } = null!;
    [JsonIgnore]
    public ILogger Logger { get; set; } = null!;
    [JsonIgnore]
    public Settings Settings { get; set; } = null!;

    private readonly Random _random = new();
    public string Name { get; init; } = null!;

    public PlaylistData PlaylistData = new();

    [JsonConstructor]
    public Playlist()
    {
    }
    
    public void LoadWithDependency(object[] parameters)
    {
		try
		{
        	Player = (IMediaPlayer)parameters[0];
        	Disk = (IDiskManager)parameters[1];
        	Logger = (ILogger)parameters[2];
        	Settings = new Settings();
		}
		catch (Exception ex)
		{
        	Logger.LogError("Error while load playlist with dependency: {ex}", ex.Message);
		}
    }

    public async Task AddTrack(Track track)
    {
        Logger.LogDebug("{TrackName} added into {playlist}", track.Metadata.TrackName, Name);
        PlaylistData.Tracks.Add(track);
        await Save();
    }

    public Playlist MergePlaylist(Playlist[] otherPlaylists)
    {
        foreach (var otherPlaylist in otherPlaylists)
            Logger.LogDebug("{Playlist1} merged with {playlist2}", Name, otherPlaylist.Name);

        var result = this;

        for (var i = 0; i < otherPlaylists.Length; i++)
            result.PlaylistData.Tracks.ForEach(track => PlaylistData.Tracks.Add(track));
        return result;
    }

    public async Task RemoveTrack(Track track)
    {
        for (var i = 0; i < PlaylistData.Tracks.Count; i++)
            if (PlaylistData.Tracks[i].TrackData.Path == track.TrackData.Path)
                PlaylistData.Tracks.Remove(PlaylistData.Tracks[i]);
        await Save();
    }

    public async Task RemoveDuplicativeTracks()
    {
        var uniquePaths = new HashSet<string>();
        var tracksToKeep = PlaylistData.Tracks.Where(track => uniquePaths.Add(track.TrackData.Path)).ToList();

        PlaylistData.Tracks = tracksToKeep;
        await Save();
    }

    public async Task SortTracks(SortPlaylistTrackFlags flags)
    {
        if (flags == SortPlaylistTrackFlags.Artist)
            PlaylistData.Tracks = PlaylistData.Tracks.OrderBy(track => track.Metadata.Artist).ToList();
        if (flags == SortPlaylistTrackFlags.ArtistInverted)
            PlaylistData.Tracks = PlaylistData.Tracks.OrderBy(track => track.Metadata.Artist).Reverse().ToList();
        if (flags == SortPlaylistTrackFlags.Year)
            PlaylistData.Tracks = PlaylistData.Tracks.OrderBy(track => track.Metadata.Year).ToList();
        if (flags == SortPlaylistTrackFlags.YearInverted)
            PlaylistData.Tracks = PlaylistData.Tracks.OrderBy(track => track.Metadata.Year).Reverse().ToList();
        if (flags == SortPlaylistTrackFlags.Durration)
            PlaylistData.Tracks = PlaylistData.Tracks.OrderBy(track => track.Metadata.Duration).ToList();
        if (flags == SortPlaylistTrackFlags.DurrationInverted)
            PlaylistData.Tracks = PlaylistData.Tracks.OrderBy(track => track.Metadata.Duration).Reverse().ToList();

        await Save();
    }


    public async Task Save()
    {
        Logger.LogDebug("Playlist saved {playlistName}", Name);
        await Disk.SavePlaylist(this);
    }

    private void UpdateLastListen()
    {
        Logger.LogDebug("Updated last listen info of playlist {playlistName}", Name);
        PlaylistData.LastListen = DateTime.Now.Date;
    }

    private void UpdateRarity(ref Track track)
    {
        Logger.LogDebug("Updated rarity of playlist {playlistName} and track {trackName} in it", Name,
            track.Metadata.TrackName);
        PlaylistData.Rarity++;
        track.IncreaseRarity(1);
    }

    public async Task Play(int startSong = 0)
    {
        var tracks = PlaylistData.Tracks;
        
        if (Settings.Avalonix.Playlists.Shuffle)
            tracks = tracks.OrderBy(_ => _random.Next()).ToList();
        
        Logger.LogDebug("Playlist {Name} has started", Name);

        for (var i = Settings.Avalonix.Playlists.Shuffle ? startSong : 0; i < tracks.Count; i++)
        {
            var track = tracks[i];

            UpdateLastListen();
            UpdateRarity(ref track);

            await Save();

            Player.Play(track);

            while (!Player.IsFree)
                await Task.Delay(1000);
        }

        if (Settings.Avalonix.Playlists.Loop) await Play();

        Logger.LogDebug("Playlist {Name} completed", Name);
    }

    public void Stop()
    {
        Logger.LogDebug("Playlist stopped");
        Player.Stop();
    }

    public void Pause()
    {
        Logger.LogDebug("Playlist paused");
        Player.Pause();
    }

    public void Resume()
    {
        Logger.LogDebug("Playlist resumed");
        Player.Resume();
    }
}