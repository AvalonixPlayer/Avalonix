using System;
using System.IO;
using Avalonix.Model.Media.Playlist;
using Microsoft.EntityFrameworkCore;

namespace Avalonix.Services.DatabaseService;

public class AppDbContext : DbContext
{
    public DbSet<PlaylistData> Playlists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var homePath = Environment.OSVersion.Platform == PlatformID.Unix ||
                       Environment.OSVersion.Platform == PlatformID.MacOSX
            ? Environment.GetEnvironmentVariable("HOME")
            : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
        var path = Path.Combine(homePath + "/.avalonix/playlists.db");
        optionsBuilder.UseSqlite($"Data Source={path}");
    }
}
