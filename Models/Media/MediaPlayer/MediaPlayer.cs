using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;
using Un4seen.Bass;

namespace Avalonix.Models.Media.MediaPlayer;

public class MediaPlayer : IMediaPlayer
{
    private int _stream;
    private float _currentVolume = 1.0f;

    public bool IsFree => Bass.BASS_ChannelIsActive(_stream) == BASSActive.BASS_ACTIVE_STOPPED;
    public bool IsPaused => Bass.BASS_ChannelIsActive(_stream) == BASSActive.BASS_ACTIVE_PAUSED;

    private readonly ILogger _logger;
    private readonly ISettingsManager _settingsManager;
    
    public Track.Track? CurrentTrack { get; private set; }

    public MediaPlayer(ILogger logger, ISettingsManager settingsManager)
    {
        _logger = logger;
        _settingsManager = settingsManager;
        
        Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
        
        Dispatcher.UIThread.Post(async void () =>
            _currentVolume = (await _settingsManager.GetSettings()).Avalonix.Volume / 100F);
    }

    public void Play(Track.Track track)
    {
        CurrentTrack = track;
        Bass.BASS_StreamFree(_stream);
        _stream = Bass.BASS_StreamCreateFile(track.TrackData.Path, 0, 0, BASSFlag.BASS_DEFAULT);
        
        if (_stream == 0)
        {
            _logger.LogError("Could not create stream {TrackDataPath}", track.TrackData.Path);
            return;
        }
        
        Bass.BASS_ChannelPlay(_stream, true);
        Bass.BASS_ChannelSetAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, _currentVolume);
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
    
    public async Task ChangeVolume(uint volume)
    {
        var settings = await _settingsManager.GetSettings();
        settings.Avalonix.Volume = volume;
        _currentVolume = volume / 100F;
        await _settingsManager.SaveSettings(settings);
        Bass.BASS_ChannelSetAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, volume / 100F);
    }

    public double GetPosition() =>
        Bass.BASS_ChannelGetPosition(_stream, BASSMode.BASS_POS_BYTE) / Bass.BASS_ChannelBytes2Seconds(_stream, 1);
    
    public void SetPosition(double position) =>
        Bass.BASS_ChannelSetPosition(_stream, Bass.BASS_ChannelSeconds2Bytes(_stream, position));
}