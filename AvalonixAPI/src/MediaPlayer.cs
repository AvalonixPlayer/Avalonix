using System;
using System.IO;
using System.Threading;
using NAudio.Utils;
using NAudio.Wave;

namespace AvalonixAPI;

public static class MediaPlayer
{
    private static WaveOutEvent _playingMusic = null!;

    public static float totalMusicTime { get; private set; } = 0;

    public static string musicName { get; private set; } = null!;

    public static void Play(string path)
    {
        using (var audioFile = new AudioFileReader(path))
        {
            musicName = Path.GetFileNameWithoutExtension(path);
            totalMusicTime = (float)audioFile.TotalTime.TotalSeconds;
            _playingMusic = new WaveOutEvent();
            _playingMusic.Init(audioFile);
            _playingMusic.Play();

            while (_playingMusic != null && Playing())
            {
                Thread.Sleep(1000);
            }
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
#if DEBUG
            Console.WriteLine("playing music is null");
#endif
        }
    }

    public static bool Playing()
    {
        return _playingMusic != null ? (_playingMusic.PlaybackState == PlaybackState.Playing ? true : false) : false;
    }

    public static float MusicTime()
    {
        return _playingMusic != null ? (float)_playingMusic.GetPositionTimeSpan().TotalSeconds : 0;
    }
}
