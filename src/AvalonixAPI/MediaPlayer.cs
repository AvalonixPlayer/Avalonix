using System.Threading;
using NAudio.Wave;

namespace Avalonix.AvalonixAPI;

public static class MediaPlayer
{
    private static Thread _playbackThread = null!;
    private static AudioFileReader _audioFile = null!;
    private static WaveOutEvent _output = null!;

    public static void Play(string path)
    {
        _playbackThread = new Thread(() =>
        {
            _audioFile = new AudioFileReader(path);
            _output = new WaveOutEvent();
            _output.Init(_audioFile);
            _output.Play();
            while (_output?.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(1000);
            }
        });
        _playbackThread.Start();
    }

    public static void Stop()
    {
        if (_output?.PlaybackState == PlaybackState.Playing)
        {
            _output.Stop();
            _output = null!;
        }
        _playbackThread = null!;
    }

    public static void Pause()
    {
        if (_output?.PlaybackState == PlaybackState.Playing)
        {
            _output.Pause();
        }
    }

    public static void Continue()
    {
        if (_output?.PlaybackState == PlaybackState.Paused)
        {
            _output.Play();
        }
    }
}