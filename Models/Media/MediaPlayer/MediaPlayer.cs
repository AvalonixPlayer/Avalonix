using System;
using System.Threading.Tasks;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;
using Un4seen.Bass;

namespace Avalonix.Models.Media.MediaPlayer;

public class MediaPlayer : IMediaPlayer
{
    private int _stream;

    public bool IsFree => Bass.BASS_ChannelIsActive(_stream) == BASSActive.BASS_ACTIVE_STOPPED;
    public bool IsPaused => Bass.BASS_ChannelIsActive(_stream) == BASSActive.BASS_ACTIVE_PAUSED;

    private readonly ILogger _logger;
    private readonly ISettingsManager _settingsManager;

    public MediaPlayer(ILogger logger, ISettingsManager settingsManager)
    {
        _logger = logger;
        _settingsManager = settingsManager;
        
        Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
    }

    public void Play(Track.Track track)
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
    
    public async Task ChangeVolume(int volume)
    {
        var settings = await _settingsManager.GetSettings();
        settings.Avalonix.Volume = volume;
        await _settingsManager.SaveSettings(settings);
        Bass.BASS_ChannelSetAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, volume / 100F);
    }
}