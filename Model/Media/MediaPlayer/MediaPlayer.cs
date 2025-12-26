using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;
using Un4seen.Bass;

namespace Avalonix.Model.Media.MediaPlayer;

public class MediaPlayer : IMediaPlayer
{
    private readonly Lock _lock = new();
    private readonly ILogger _logger;
    private readonly ISettingsManager _settingsManager;
    private int _stream;
    private BASS_DX8_PARAMEQ _eqPar = new();
    private int[] _fx = [0, 1, 2];

    public MediaPlayer(ILogger logger, ISettingsManager settingsManager)
    {
        _logger = logger;
        _settingsManager = settingsManager;
        Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
    }

    private void InitFX()
    {
        for (var i = 0; i < _fx.Length; i++)
        {
            _fx[i] = Bass.BASS_ChannelSetFX(_stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
        }
    }

    public void SetParametersEQ(int fx, int center, float gain)
    {
        _eqPar.fBandwidth = 18.0f;
        _eqPar.fCenter = center;
        _eqPar.fGain = gain;
        Bass.BASS_FXSetParameters(fx, _eqPar);
    }

    public bool IsFree => Bass.BASS_ChannelIsActive(_stream) == BASSActive.BASS_ACTIVE_STOPPED;
    public bool IsPaused => Bass.BASS_ChannelIsActive(_stream) == BASSActive.BASS_ACTIVE_PAUSED;

    public Track.Track? CurrentTrack { get; private set; }

    public event Action<bool>? PlaybackStateChanged;
    public event Action? TrackChanged;

    public void Play(Track.Track track)
    {
        lock (_lock)
        {
            CurrentTrack = track;
            Bass.BASS_StreamFree(_stream);
            try
            {
                _stream = Bass.BASS_StreamCreateFile(track.TrackData.Path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT);
            }
            catch (Exception e)
            {
                _logger.LogError("Create stream error: {e}", e.Message);
            }

            if (_stream == 0)
            {
                _logger.LogError("Could not create stream {TrackDataPath}", track.TrackData.Path);
                return;
            }

            InitFX();
            
            Bass.BASS_ChannelPlay(_stream, true);
            ChangeVolume(_settingsManager.Settings!.Avalonix.Volume);
            SetParametersEQ(_fx[0], 100, -5);
            SetParametersEQ(_fx[1], 1000, 0);
            SetParametersEQ(_fx[2], 8000, 15);
            _logger.LogInformation("Now playing {MetadataTrackName}", track.Metadata.TrackName);

            PlaybackStateChanged?.Invoke(false);
            TrackChanged?.Invoke();
        }
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

    public Task ChangeVolume(uint volume)
    {
        var vol = _settingsManager.Settings!.Avalonix.Volume = volume;
        Bass.BASS_ChannelSetAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, vol / 100F);
        return Task.CompletedTask;
    }

    public double GetPosition()
    {
        return Bass.BASS_ChannelBytes2Seconds(_stream, Bass.BASS_ChannelGetPosition(_stream, BASSMode.BASS_POS_BYTE));
    }

    public void SetPosition(double position)
    {
        Bass.BASS_ChannelSetPosition(_stream, Bass.BASS_ChannelSeconds2Bytes(_stream, position));
    }

    public void Reset()
    {
        PlaybackStateChanged?.Invoke(false);
        Bass.BASS_ChannelPlay(_stream, true);
    }
}