using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Playlist;
using Avalonix.Models.UserSettings.AvalonixSettingsFiles;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Media.Album;

public record Album
{
    public AlbumMetadata? Metadata { get; private set; }
    public AlbumData? AlbumData { get; private set; }
    public PlayQueue PlayQueue { get; }

    public Album(List<string> tracksPaths, IMediaPlayer player, ILogger logger, PlaySettings settings)
    {
        PlayQueue = new PlayQueue(player, logger, settings);

        AlbumData = new AlbumData(tracksPaths);
        Metadata = new AlbumMetadata(tracksPaths);
        
        PlayQueue.FillQueue(AlbumData.Tracks);
    }
}