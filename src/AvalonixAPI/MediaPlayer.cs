using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Dsp;
using NAudio.Utils;
using NAudio.Wave;
namespace AvalonixAPI
{
    public static class MediaPlayer
    {
        private static WaveOutEvent _playingMusic = null!;

        private static float _totalMusicTime = 0;
        private static string _musicName = null!;
        public static void Play(string path)
        {
            using (var audioFile = new AudioFileReader(path))
            {
                _musicName = Path.GetFileName(path);
                _totalMusicTime = (float)audioFile.TotalTime.TotalSeconds;
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

        public static float TotalMusicTime()
        {
            return _totalMusicTime;
        }

        public static string MusicName()
        {
            return _musicName;
        }
    }
}