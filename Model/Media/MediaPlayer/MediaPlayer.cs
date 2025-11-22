using System;
using System.Threading.Tasks;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;
using Un4seen.Bass;

namespace Avalonix.Services.Media.MediaPlayer;

public class MediaPlayer : IMediaPlayer
{
    private int _stream;
    private readonly ILogger _logger;
    private readonly ISettingsManager _settingsManager;

    public bool IsFree => Bass.BASS_ChannelIsActive(_stream) == BASSActive.BASS_ACTIVE_STOPPED;
    public bool IsPaused => Bass.BASS_ChannelIsActive(_stream) == BASSActive.BASS_ACTIVE_PAUSED;

    public Track.Track? CurrentTrack { get; private set; }

    public event Action<bool>? PlaybackStateChanged;
    public event Action? TrackChanged;

    public MediaPlayer(ILogger logger, ISettingsManager settingsManager)
    {
        _logger = logger;
        _settingsManager = settingsManager;
        Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
    }

    public void Play(Track.Track track)
    {
        CurrentTrack = track;
        Bass.BASS_StreamFree(_stream);
        try
        {
            _stream = Bass.BASS_StreamCreateFile(track.TrackData.Path, 0, 0, BASSFlag.BASS_DEFAULT);
        }
        catch (Exception e)
        {
            _logger.LogError("Create stream error: {e}",e.Message);
        }

        if (_stream == 0)
        {
            _logger.LogError("Could not create stream {TrackDataPath}", track.TrackData.Path);
            return;
        }

        Bass.BASS_ChannelPlay(_stream, true);
        ChangeVolume(_settingsManager.Settings!.Avalonix.Volume);
        _logger.LogInformation("Now playing {MetadataTrackName}", track.Metadata.TrackName);

        PlaybackStateChanged?.Invoke(false);
        TrackChanged?.Invoke();
    }

    public void Stop()
    {
        Bass.BASS_ChannelFree(_stream);
        Bass.BASS_StreamFree(_stream);
    }

    public void Pause()
    {
        Bass.BASS_ChannelPause(_stream);
        PlaybackStateChanged?.Invoke(true);
    }

    public void Resume()
    {
        Bass.BASS_ChannelPlay(_stream, false);
        PlaybackStateChanged?.Invoke(false);
    }

    public void Reset()
    {
        PlaybackStateChanged?.Invoke(false);
        Bass.BASS_ChannelPlay(_stream, true);
    }

    public Task ChangeVolume(uint volume)
    {
        var vol = _settingsManager.Settings!.Avalonix.Volume = volume;
        Bass.BASS_ChannelSetAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, vol / 100F);
        return Task.CompletedTask;
    }

    public double GetPosition() =>
        Bass.BASS_ChannelBytes2Seconds(_stream, Bass.BASS_ChannelGetPosition(_stream, BASSMode.BASS_POS_BYTE));

    public void SetPosition(double position) =>
        Bass.BASS_ChannelSetPosition(_stream, Bass.BASS_ChannelSeconds2Bytes(_stream, position));
}