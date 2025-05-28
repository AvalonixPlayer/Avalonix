namespace AvalonixAPI;

public class Playlist
{
    private readonly string[] _audios;

    public Playlist(string playlistName) => _audios = PlaylistsManager.GetAudios(playlistName);
    public void Play()
    {
        Thread thread = new Thread(Thr);
        thread.Start();
        return;

        void Thr()
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