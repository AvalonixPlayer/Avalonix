using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaAPI
{
    public class Playlist
    {
        private string _playlistName = null!;

        public Playlist(string playlistName)
        {
            _playlistName = playlistName;
        }

        public void Play()
        {
            foreach (var i in DiskManager.GetAudios(_playlistName))
            {
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

        public void Stop()
        {
            MediaPlayer.Stop();
        }
    }
}