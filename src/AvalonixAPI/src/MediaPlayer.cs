using System.IO;
using System.Threading;
using NAudio.Utils;
using NAudio.Wave;
using NeoSimpleLogger;


namespace AvalonixAPI;

public static class MediaPlayer
{
    public static Logger Logger { get; } = new(Logger.TypeLogger.Console);
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
            Logger.CallStack = false;
            Logger.Info($"{nameof(MediaPlayer)} is stopped.");
        }
    }

    public static bool Playing() => _playingMusic.PlaybackState == PlaybackState.Playing;

    public static float MusicTime() => (float)_playingMusic.GetPositionTimeSpan().TotalSeconds;
}
