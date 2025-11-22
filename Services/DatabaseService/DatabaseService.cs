using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Services.Media.Playlist;
using Microsoft.EntityFrameworkCore;

namespace Avalonix.Services.DatabaseService;

public class DatabaseService(AppDbContext dbContext) : IDatabaseService
{
    public async Task WritePlaylist(Playlist playlist) => await dbContext.AddAsync(playlist);

    public Task RemovePlaylist(Playlist playlist)
    {
        dbContext.Remove(playlist);
        return Task.CompletedTask;
    }

    public Task<List<Playlist>> GetAllPlaylists() => dbContext.Playlists.ToListAsync();
}
