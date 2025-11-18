using System.Collections.Generic;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.Media.Playlist;
using Avalonix.Models.UserSettings.AvalonixSettingsFiles;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Media.Album;

public record Album
{
    public AlbumMetadata? Metadata;
    private AlbumData? _albumData;
    public PlayQueue PlayQueue { get; }

    public Album(List<string> tracksPaths, IMediaPlayer player, ILogger logger, PlaySettings settings)
    {
        PlayQueue = new PlayQueue(player, logger, settings);

        Metadata = new AlbumMetadata(tracksPaths);
        _albumData = new AlbumData(tracksPaths);
        
        PlayQueue.FillQueue(_albumData.Tracks);
    }
}