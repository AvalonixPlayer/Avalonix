using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Playlist;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services;

public class PlaylistManager(IDiskManager diskManager, IMediaPlayer player, ILogger<PlaylistManager> logger) : IPlaylistManager
{
    private readonly Lock _lock = new();
    
    public List<Playlist> Playlists { get; set; } = [];
    public Playlist? PlayingPlaylist { get; private set; }
    
    public async Task DeletePlaylistAsync(Playlist playlist)
    {
        if (PlayingPlaylist == playlist)
        {
            await StopPlaylistAsync();
        }
        
        await diskManager.RemovePlaylist(playlist.Name);
        Playlists.Remove(playlist);
    }

    public async Task SavePlaylistAsync(Playlist playlist)
    {
        await diskManager.SavePlaylist(playlist);
        
        var existingIndex = Playlists.FindIndex(p => p.Name == playlist.Name);
        if (existingIndex >= 0)
        {
            Playlists[existingIndex] = playlist;
        }
        else
        {
            Playlists.Add(playlist);
        }
    }

    public Task EditPlaylistAsync(Playlist playlist) => SavePlaylistAsync(playlist);

    public async Task StartPlaylistAsync(Playlist playlist)
    {
        lock (_lock)
        {
            if (PlayingPlaylist == playlist)
            {
                logger.LogInformation("Playlist {PlaylistName} is already playing", playlist.Name);
                return;
            }

            if (PlayingPlaylist != null && PlayingPlaylist != playlist)
            {
                PlayingPlaylist.Stop();
            }

            PlayingPlaylist = playlist;
        }

        try
        {
            await playlist.Play();
            
            logger.LogInformation("Playlist {PlaylistName} finished playing", playlist.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error playing playlist {PlaylistName}", playlist.Name);
            throw;
        }
        finally
        {
            lock (_lock)
            {
                if (PlayingPlaylist == playlist)
                {
                    PlayingPlaylist = null;
                }
            }
        }
    }

    public Task StopPlaylistAsync()
    {
        lock (_lock)
        {
            if (PlayingPlaylist == null) return Task.CompletedTask;
            var playlistName = PlayingPlaylist.Name;
            PlayingPlaylist.Stop();
            PlayingPlaylist = null;
            logger.LogInformation("Stopped playlist {PlaylistName}", playlistName);
        }
        
        return Task.CompletedTask;
    }

    public Playlist ConstructPlaylist(string name, PlaylistData playlistData) => new(name, playlistData, player, diskManager, logger);

    public Task PausePlaylistAsync()
    {
        lock (_lock)
        {
            PlayingPlaylist?.Pause();
        }
        return Task.CompletedTask;
    }

    public Task ResumePlaylistAsync()
    {
        lock (_lock)
        {
            PlayingPlaylist?.Resume();
        }
        return Task.CompletedTask;
    }
}