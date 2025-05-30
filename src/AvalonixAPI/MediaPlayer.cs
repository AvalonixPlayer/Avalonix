using System.IO;
using System.Threading;
using Avalonix;
using NAudio.Utils;
using NAudio.Wave;
using static Avalonix.Program;


namespace AvalonixAPI;

public static class MediaPlayer
{
    private static WaveOutEvent _playingMusic = null!;

    public static float totalMusicTime { get; private set; } = 0;

    public static string musicName { get; private set; } = null!;

    public static void Play(string path)
    {
        using var audioFile = new AudioFileReader(path);
        musicName = Path.GetFileNameWithoutExtension(path);
        totalMusicTime = (float)audioFile.TotalTime.TotalSeconds;
        _playingMusic = new WaveOutEvent();
        _playingMusic.Init(audioFile);
        _playingMusic.Play();
        Logger.Info($"{nameof(audioFile)} is started");

        while (Playing())
        {
            Thread.Sleep(1000);
        }
    }

    public static void Stop()
    {
        _playingMusic.Stop();
        _playingMusic = null!;
        Logger.Info($"{nameof(MediaPlayer)} is stopped.");
    }

    public static bool Playing() => _playingMusic.PlaybackState == PlaybackState.Playing;

    public static float MusicTime() => (float)_playingMusic.GetPositionTimeSpan().TotalSeconds;

    public static void Pause()  
    {
        _playingMusic.Pause();
        Logger.Info($"{nameof(MediaPlayer)} is paused.");
    }
}
