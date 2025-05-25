using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Dsp;
using NAudio.Wave;
namespace AvalonixAPI
{
    public static class MediaPlayer
    {
        private static WaveOutEvent _playingMusic = null!;
        public static void Play(string path)
        {
            using (var audioFile = new AudioFileReader(path))
            {
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
    }
}