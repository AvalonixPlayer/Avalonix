using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Model.Media.Playlist;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.DatabaseService;

public class DatabaseService : IDatabaseService
{
    private readonly AppDbContext _dbContext;

    public DatabaseService(AppDbContext dbContext, ILogger logger)
    {
        _dbContext = dbContext;

        try
        {
            if (!_dbContext.Database.EnsureCreated())
            {
                logger.LogWarning("Table 'Playlists' already exist");
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error while initializing: {ExMessage}", ex.Message);
        }
    }

    public async Task WritePlaylistData(PlaylistData playlist) => await _dbContext.AddAsync(playlist);

    public void RemovePlaylistData(PlaylistData playlist) => _dbContext.Remove(playlist);

    public void RemovePlaylistData(string plName) => _dbContext.Remove(new PlaylistData { Name = plName});

    public Task<List<PlaylistData>> GetAllPlaylists() => _dbContext.Playlists.ToListAsync();
}
