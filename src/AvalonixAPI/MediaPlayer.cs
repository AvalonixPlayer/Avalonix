using System;
using System.IO;
using System.Threading;
using NAudio.Utils;
using NAudio.Wave;
using NeoSimpleLogger;

namespace Avalonix.AvalonixAPI;
public static class MediaPlayer
{
    public Logger _logger = new Logger();
    private static WaveOutEvent _playingMusic = null!;

    private static float _totalMusicTime;
    private static string _musicName = null!;
    public static void Play(string path)
    {
        using var audioFile = new AudioFileReader(path);
        _musicName = Path.GetFileName(path);
        _totalMusicTime = (float)audioFile.TotalTime.TotalSeconds;
        _playingMusic = new WaveOutEvent();
        _playingMusic.Init(audioFile);
        _playingMusic.Play();
        _logger.Info($"Start playing {path}");

        while (Playing())
        {
            Thread.Sleep(1000);
        }
    }

    public static void Stop()
    {
        if (_playingMusic != null)
        {
            _playingMusic.Stop();
            _playingMusic = null!;
        }
        else
        {
            _logger.Error("Playing music is null");
        }
    }

    public static bool Playing()
    {
        return _playingMusic.PlaybackState == PlaybackState.Playing;
    }

    public static float MusicTime()
    {
        return _playingMusic != null ? (float)_playingMusic.GetPositionTimeSpan().TotalSeconds : 0;
    }

    public static float TotalMusicTime()
    {
        return _totalMusicTime;
    }

    public static string MusicName()
    {
        return _musicName;
    }
}
