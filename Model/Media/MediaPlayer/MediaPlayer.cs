using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;
using Un4seen.Bass;

namespace Avalonix.Model.Media.MediaPlayer;

public class MediaPlayer : IMediaPlayer
{
    private readonly BASS_DX8_PARAMEQ _eqPar = new();
    private readonly int[] _fx = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16];
    private readonly Lock _lock = new();
    private readonly ILogger _logger;
    private readonly ISettingsManager _settingsManager;
    private int _stream;

    public MediaPlayer(ILogger logger, ISettingsManager settingsManager)
    {
        _logger = logger;
        _settingsManager = settingsManager;
        Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
    }

    public void SetParametersEQ(int fxIndex, int center, float gain)
    {
        _eqPar.fBandwidth = 18.0f;
        _eqPar.fCenter = center;
        _eqPar.fGain = gain;
        Bass.BASS_FXSetParameters(_fx[fxIndex], _eqPar);
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

            SetParametersEQ(0, 64, _settingsManager.Settings.Avalonix.EqualizerSettings._fxs[0]);
            SetParametersEQ(1, 125, _settingsManager.Settings.Avalonix.EqualizerSettings._fxs[1]);
            SetParametersEQ(2, 250, _settingsManager.Settings.Avalonix.EqualizerSettings._fxs[2]);
            SetParametersEQ(3, 500, _settingsManager.Settings.Avalonix.EqualizerSettings._fxs[3]);
            SetParametersEQ(4, 1000, _settingsManager.Settings.Avalonix.EqualizerSettings._fxs[4]);
            SetParametersEQ(5, 4000, _settingsManager.Settings.Avalonix.EqualizerSettings._fxs[5]);
            Bass.BASS_ChannelPlay(_stream, true);
            ChangeVolume(_settingsManager.Settings!.Avalonix.Volume);
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

    private void InitFX()
    {
        for (var i = 0; i < _fx.Length; i++)
            _fx[i] = Bass.BASS_ChannelSetFX(_stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
    }

    public void Reset()
    {
        PlaybackStateChanged?.Invoke(false);
        Bass.BASS_ChannelPlay(_stream, true);
    }
}