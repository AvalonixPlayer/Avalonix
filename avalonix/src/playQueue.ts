import { invoke } from "@tauri-apps/api/core";
import { Track } from "./bindings/Track";

export async function addTrackToQueue(track: Track) {
  await invoke('add_track_to_queue', {track: track});
  await UpdateTrackQueueUI();
}

const pickBtnTempl = document.querySelector('#track-btn-example') as HTMLTemplateElement;

export async function UpdateTrackQueueUI() {
  console.log(1);
  var tracks = await invoke<Array<Track>>('get_queue');
  console.log(2);

  const container = document.getElementById("play-queue");
  if (!container || !pickBtnTempl) return;
  console.log(3);

  container.innerHTML = "";

  let index = 0;
  tracks.forEach(track => {
    index++;
    const clone = createTrackBtn(track, index);
    container.appendChild(clone);
  });
}

function createTrackBtn(track: Track, index: number): DocumentFragment {
    const clone = pickBtnTempl.content.cloneNode(true) as DocumentFragment;
    
    const trackNameClone = clone.querySelector("h5");
    const artistNameClone = clone.querySelector("h6");
    const playBtn = clone.querySelector('.track-btn-play');
    
    playBtn!.addEventListener("click", (_) => {
        console.log('Let`s play track: ' + track.metadata.title)
    });

    var title = track.metadata.title || "Unknown Title";
    var artist = track.metadata.artist || "Unknown Artist";

    if(title.length > 35) {
        title = title.slice(0,35);
        title = title + "...";
    }
    title = index + ". " + title;
    if(artist.length > 20) {
        artist = artist.slice(0,35);
        artist = artist + "...";
    }

    if (trackNameClone) trackNameClone.textContent = title;
    if (artistNameClone) artistNameClone.textContent = artist;
    return clone;
}