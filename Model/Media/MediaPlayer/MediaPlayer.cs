using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace Avalonix.Model.Media.MediaPlayer;

public class MediaPlayer : IMediaPlayer
{
    private readonly Lock _lock = new();
    private readonly ILogger _logger;
    private readonly ISettingsManager _settingsManager;
    private int _stream;
    private int _fxEQ;

    public MediaPlayer(ILogger logger, ISettingsManager settingsManager)
    {
        _logger = logger;
        _settingsManager = settingsManager;
        Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
    }

    private void SetBFX_EQ()
    {
        // set peaking equalizer effect with no bands
        _fxEQ = Bass.BASS_ChannelSetFX(_stream, BASSFXType.BASS_FX_BFX_PEAKEQ, 0);

        // setup the EQ bands
        BASS_BFX_PEAKEQ eq = new BASS_BFX_PEAKEQ();
        eq.fQ = 0f;
        eq.fBandwidth = 2.5f;
        eq.lChannel = BASSFXChan.BASS_BFX_CHANALL;

        // create 1st band for bass
        eq.lBand = 0;
        eq.fCenter = 125f;
        Bass.BASS_FXSetParameters(_fxEQ, eq);
        UpdateFX(0, 0f);

        // create 2nd band for mid
        eq.lBand = 1;
        eq.fCenter = 1000f;
        Bass.BASS_FXSetParameters(_fxEQ, eq);
        UpdateFX(1, 0f);

        // create 3rd band for treble
        eq.lBand = 2;
        eq.fCenter = 8000f;
        Bass.BASS_FXSetParameters(_fxEQ, eq);
        UpdateFX(2, 0f);
    }

    private void UpdateFX(int band, float gain)
    {
        var eq = new BASS_BFX_PEAKEQ();
        // get values of the selected band
        eq.lBand = band;
        Bass.BASS_FXGetParameters(_fxEQ, eq);
        eq.fGain = gain;
        Bass.BASS_FXSetParameters(_fxEQ, eq);
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
                _stream = Bass.BASS_StreamCreateFile(track.TrackData.Path, 0, 0, BASSFlag.BASS_DEFAULT);
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

    public void Reset()
    {
        PlaybackStateChanged?.Invoke(false);
        Bass.BASS_ChannelPlay(_stream, true);
    }
}