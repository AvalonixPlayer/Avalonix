using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Model.Media.Playlist;
using Microsoft.EntityFrameworkCore;

namespace Avalonix.Services.DatabaseService;

public class DatabaseService(AppDbContext dbContext) : IDatabaseService
{
    public async Task WritePlaylistData(PlaylistData playlist) => await dbContext.AddAsync(playlist);

    public void RemovePlaylistData(PlaylistData playlist) => dbContext.Remove(playlist);

    public void RemovePlaylistData(string plName, List<PlaylistData> playlists)
    {
        var playlist = playlists.FirstOrDefault(playlist1 => playlist1.Name == plName);
        if (playlist != null) RemovePlaylistData(playlist);
    }

    public Task<List<PlaylistData>> GetAllPlaylists() => dbContext.Playlists.ToListAsync();
}
