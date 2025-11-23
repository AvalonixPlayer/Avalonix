using System.Threading.Tasks;
using Avalonix.Model.Media.Playlist;

namespace Avalonix.Model.Media.PlayBox;

public class PlayBox : IPlayable
{
    public string Name => "PlayBox";
    public PlayQueue PlayQueue { get; }

    public async Task Play() =>
        await PlayQueue.Play().ConfigureAwait(false);

    public void Pause() =>
        PlayQueue.Pause();

    public void Stop() =>
        PlayQueue.Stop();

    public void Resume() =>
        PlayQueue.Resume();

    public void NextTrack() =>
        PlayQueue.NextTrack();

    public void BackTrack() =>
        PlayQueue.BackTrack();

    public void ForceStartTrackByIndex(int index) =>
        PlayQueue.ForceStartTrackByIndex(index);

    public Task LoadTracksMetadata()
    {
        _ = Task.Run(() =>
        {
            foreach (var i in PlayQueue.Tracks)
            {
                i.Metadata.Init(i.TrackData.Path);
                i.Metadata.FillTrackMetaData();
            }
        });
        return Task.CompletedTask;
    }

    public bool QueueIsEmpty() =>
        PlayQueue.QueueIsEmpty();
}