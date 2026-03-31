import { invoke } from "@tauri-apps/api/core";
import { Track } from "./bindings/Track";

export async function addTrackToQueue(track: Track) {
  await invoke("add_track_to_queue", { track: track });
  await UpdateTrackQueueUI();
}

const pickBtnTempl = document.querySelector(
  "#track-btn-example",
) as HTMLTemplateElement;

const searcher = document.getElementById(
  "track-in-queue-search-area",
) as HTMLInputElement;
searcher.addEventListener("input", async (_) => {
  await UpdateTrackQueueUI();
});

export async function UpdateTrackQueueUI() {
  const container = document.getElementById("play-queue");
  if (!container || !pickBtnTempl) return;

  const start = performance.now();
  const tracks = await invoke<Array<Track>>("get_queue");
  var end = performance.now();
  console.log(`Время выполнения передачи: ${end - start} миллисекунд`);

  const filter = searcher.value.toLowerCase();

  const fragment = document.createDocumentFragment();

  container.innerHTML = "";

  const filteredTracks = tracks.filter((track) => {
    if (!filter) return true;
    const title = track.metadata.title?.toLowerCase() || "";
    return title.includes(filter);
  });

  let index = 1;
  for (const track of filteredTracks) {
    const clone = await createTrackBtn(track, index++);
    fragment.appendChild(clone);
  }

  container.appendChild(fragment);
  var end = performance.now();
  console.log(`Время выполнения заполнения: ${end - start} миллисекунд`);
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

async function createTrackBtn(
  track: Track,
  index: number,
): Promise<DocumentFragment> {
  const clone = pickBtnTempl.content.cloneNode(true) as DocumentFragment;

  const trackNameClone = clone.querySelector("h5");
  const artistNameClone = clone.querySelector("h6");
  const playBtn = clone.querySelector(".track-btn-play");
  const removeBtn = clone.querySelector(".track-btn-remove");

  playBtn!.addEventListener("click", (_) => {
    console.log("Let`s play track: " + track.metadata.title);
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

  if (title.length > 35) {
    title = title.slice(0, 35);
    title = title + "...";
  }
  title = index + ". " + title;
  if (artist.length > 20) {
    artist = artist.slice(0, 35);
    artist = artist + "...";
  }

  if (trackNameClone) trackNameClone.textContent = title;
  if (artistNameClone) artistNameClone.textContent = artist;
  return clone;
}
