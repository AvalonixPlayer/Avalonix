import { invoke } from "@tauri-apps/api/core";
import { Track } from "./bindings/Track";

export async function addTrackToQueue(track: Track) {
  await invoke("add_track_to_queue", { track: track });
  await UpdateTrackQueueUI();
}

const pickBtnTempl = document.querySelector(
  "#track-btn-example",
) as HTMLTemplateElement;

export async function UpdateTrackQueueUI() {
  const container = document.getElementById("play-queue");
  if (!container || !pickBtnTempl) return;

  const tracks = await invoke<Array<Track>>("get_queue");

  const fragment = document.createDocumentFragment();

  container.innerHTML = "";

  for (const track of tracks) {
    const clone = await createTrackBtn(track);
    fragment.appendChild(clone);
  }

  container.appendChild(fragment);
}

async function ClearQueue() {
  await invoke("clear_queue");
  await UpdateTrackQueueUI();
}

(async () => {
  document
    .getElementById("clear-queue")!
    .addEventListener("click", async (_) => {
      await ClearQueue();
    });
})();

async function createTrackBtn(track: Track): Promise<DocumentFragment> {
  const clone = pickBtnTempl.content.cloneNode(true) as DocumentFragment;

  const trackNameClone = clone.querySelector("h5");
  const artistNameClone = clone.querySelector("h6");
  const playBtn = clone.querySelector(".track-btn-play");
  const removeBtn = clone.querySelector(".track-btn-remove");

  playBtn!.addEventListener("click", async (_) => {
    await invoke("play_track", { track: track });
  });

  removeBtn!.addEventListener("click", async (_) => {
    console.log(track.file_path);

    await invoke("remove_track_from_queue", {
      trackPath: track.file_path,
    });
    await UpdateTrackQueueUI();
  });

  var title = track.metadata.title || "Unknown Title";
  var artist = track.metadata.artist || "Unknown Artist";

  if (trackNameClone) trackNameClone.textContent = title;
  if (artistNameClone) artistNameClone.textContent = artist;
  return clone;
}
