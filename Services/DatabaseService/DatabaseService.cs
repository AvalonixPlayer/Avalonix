using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Services.Media.Playlist;
using Microsoft.EntityFrameworkCore;

namespace Avalonix.Services.DatabaseService;

public class DatabaseService(AppDbContext dbContext) : IDatabaseService
{
    public async Task WritePlaylist(Playlist playlist) => await dbContext.AddAsync(playlist);

    public void RemovePlaylist(Playlist playlist) => dbContext.Remove(playlist);

    public void RemovePlaylist(string plName, List<Playlist> playlists)
    {
        var playlist = playlists.FirstOrDefault(playlist1 => playlist1.Name == plName);
        if (playlist != null) RemovePlaylist(playlist);
    }

    public Task<List<Playlist>> GetAllPlaylists() => dbContext.Playlists.ToListAsync();
}
