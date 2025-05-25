using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvalonixAPI
{
    public class Playlist
    {
        private string _playlistName = null!;

        private string[] _audios = null!;

        public Playlist(string playlistName)
        {
            _playlistName = playlistName;
            _audios = DiskManager.GetAudios(_playlistName);
        }

        public void Play()
        {

            do
            {
                foreach (var i in _audios)
                {
                    if (Settings.shuffle)
                        Shuffle();

                    MediaPlayer.Stop();
                    Thread thread = new Thread(() => MediaPlayer.Play(i));
                    thread.Start();
                    Thread.Sleep(1000);
                    while (MediaPlayer.Playing())
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            while (Settings.loopingPlaylists);
        }

        public void Shuffle()
        {
            Random.Shared.Shuffle(_audios);
        }

        public void Stop()
        {
            MediaPlayer.Stop();
        }
    }
}