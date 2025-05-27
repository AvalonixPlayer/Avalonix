using System;
using System.Threading;
using AvalonixAPI;

namespace AvalonixAPI;

public class Playlist
{
    private string _playlistName = null!;

    private readonly string[] _audios = null!;

    public Playlist(string playlistName)
    {
        _playlistName = playlistName;
        _audios = PlaylistsManager.GetAudios(_playlistName);
    }
    public void Play()
    {
        Thread thread = new Thread(() => thr());
        thread.Start();
        void thr()
        {
            do
            {
                foreach (var i in _audios)
                {
                    if (Settings.Shuffle)
                        Shuffle();

                    MediaPlayer.Stop();
                    var thread = new Thread(() => MediaPlayer.Play(i));
                    thread.Start();
                    Thread.Sleep(1000);
                    while (MediaPlayer.Playing())
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            while (Settings.LoopingPlaylists);
        }
    }

    public void Shuffle() => Random.Shared.Shuffle(_audios);

    public void Stop() => MediaPlayer.Stop();
}