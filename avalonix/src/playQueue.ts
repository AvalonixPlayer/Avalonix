import { invoke } from "@tauri-apps/api/core";
import { allTracksFilterData } from "./playboxes";
import { FilterData } from "./bindings/FilterData";
import { Track } from "./bindings/Track";

export async function addTrackToQueue(id: Array<number>) {
  await invoke("add_track_to_queue", { id: id });
  await UpdateTrackQueueUI();
}

const pickBtnTempl = document.querySelector(
  "#track-btn-example",
) as HTMLTemplateElement;

let observer: IntersectionObserver;
const container = document.getElementById("play-queue");

export async function UpdateTrackQueueUI() {
  if (!container || !pickBtnTempl) return;

  const tracksIds = await invoke<Array<Array<number>>>("get_queue");

  const fragment = document.createDocumentFragment();

  container.innerHTML = "";

  observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          const element = entry.target as HTMLElement;
          const trackId = JSON.parse(element.dataset.trackId!);

          observer.unobserve(element);

          invoke<Track>("get_track_by_id", { id: trackId }).then((track) => {
            fillTrackBtn(element, track, trackId);
            element.dataset.llCompleate = "true";
          });
        }
      });
    },
    {
      root: container,
      threshold: 0.1,
    },
  );

  for (const id of tracksIds) {
    const clone = await createTrackBtn(id);
    if (clone != null) fragment.appendChild(clone);
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

async function createTrackBtn(
  id: Array<number>,
): Promise<DocumentFragment | null> {
  let trackFilterData = allTracksFilterData.find((x) => {
    return JSON.stringify(x.id) == JSON.stringify(id);
  }) as FilterData;

  if (trackFilterData == null) return null;

  const fragment = pickBtnTempl.content.cloneNode(true) as DocumentFragment;
  const element = fragment.firstElementChild as HTMLElement;

  element.dataset.trackId = JSON.stringify(id);

  observer.observe(element);
  return fragment;
}

async function fillTrackBtn(
  element: HTMLElement,
  track: Track,
  id: Array<number>,
) {
  const trackName = element.querySelector("h5");
  const artistName = element.querySelector("h6");
  const playBtn = element.querySelector(".track-btn-play");
  const removeBtn = element.querySelector(".track-btn-remove");

  playBtn!.addEventListener("click", async (_) => {
    await invoke("play_track", { id: id });
  });

  removeBtn!.addEventListener("click", async (_) => {
    await invoke("remove_track_from_queue", {
      id: id,
    });
    await UpdateTrackQueueUI();
  });

  if (trackName) trackName.textContent = track.metadata.title;
  if (artistName) artistName.textContent = track.metadata.artist;
}
