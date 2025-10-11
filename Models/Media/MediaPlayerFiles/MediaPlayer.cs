using System;
using Avalonix.Models.Media.TrackFiles;
using Microsoft.Extensions.Logging;
using Un4seen.Bass;

namespace Avalonix.Models.Media.MediaPlayerFiles;

public class MediaPlayer : IMediaPlayer
{
    private int _stream;

    public bool IsFree => Bass.BASS_ChannelIsActive(_stream) == BASSActive.BASS_ACTIVE_STOPPED;

    private readonly ILogger _logger;

    public MediaPlayer(ILogger logger)
    {
        _logger = logger;
        Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
    }

    public void Play(Track track)
    {
        Bass.BASS_StreamFree(_stream);
        _stream = Bass.BASS_StreamCreateFile(track.TrackData.Path, 0, 0, BASSFlag.BASS_DEFAULT);
        
        if (_stream == 0)
        {
            _logger.LogError("Could not create stream {TrackDataPath}", track.TrackData.Path);
            return;
        }
        
        Bass.BASS_ChannelPlay(_stream, true);
        
        _logger.LogInformation("Now playing {MetadataTrackName}", track.Metadata.TrackName);
    }

    public void Stop()
    {
        Bass.BASS_ChannelFree(_stream);
        Bass.BASS_StreamFree(_stream);
    }

    public void Pause() =>
        Bass.BASS_ChannelPause(_stream);

    public void Resume() =>
        Bass.BASS_ChannelPlay(_stream, false);

    public void Reset() =>
        Bass.BASS_ChannelPlay(_stream, true);
    
    public void ChangeVolume(int volume) => // the volume should be in the range from 0 to 100
        Bass.BASS_ChannelSetAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, volume / 100F);
}