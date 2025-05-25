using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsolePlayer.src
{
    public class Playlist
    {
        private string[] _audioPaths = null!;

        public void GetAudios(string playlistName)
        {
            _audioPaths = Directory.GetFiles(Path.GetDirectoryName(Environment.ProcessPath) + "\\" + playlistName);
        }

        public void Play()
        {
            foreach (var i in _audioPaths)
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
            _audioPaths = new string[0];
            MediaPlayer.Stop();
        }
    }
}