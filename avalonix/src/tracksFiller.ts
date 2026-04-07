import type { Track } from "./bindings/Track";
import { invoke } from "@tauri-apps/api/core";
import { addTrackToQueue } from "./playQueue";
import { allTracksFilterData, allTracksId } from "./playboxes";

let observer: IntersectionObserver;

export async function tracksFillerInit() {
  await fillTracksList();
}

const container = document.getElementById("tracks-list");
const pickBtnTempl = document.querySelector(
  "#pick-track-btn-example",
) as HTMLTemplateElement;

async function fillTracksList() {
  if (!container || !pickBtnTempl) return;

  container.innerHTML = "";

  observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          const element = entry.target as HTMLElement;
          const trackId = JSON.parse(element.dataset.trackId!);

          observer.unobserve(element);

          invoke<Track>("get_track_by_id", { id: trackId }).then((track) => {
            fillPickBtn(element, track, trackId);
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

  let searchLine = document.getElementById(
    "track-search-area",
  ) as HTMLInputElement;

  searchLine!.addEventListener("input", async (_) => {
    fill(searchLine.value);
  });
  fill("");

  function fill(query: string) {
    let filteredId = getFilteredIds(query);
    container!.innerHTML = "";
    filteredId.forEach((id) => {
      const fragment = pickBtnTempl.content.cloneNode(true) as DocumentFragment;
      const element = fragment.firstElementChild as HTMLElement;
      element.dataset.trackId = JSON.stringify(id);

      container!.append(fragment);
      observer.observe(element);
    });
  }
}

function getFilteredIds(searchQuery: string): Array<Array<number>> {
  let result = new Array<Array<number>>();
  for (let i = 0; i < allTracksFilterData.length; i++) {
    if (
      allTracksFilterData[i].name
        .toLowerCase()
        .includes(searchQuery.toLowerCase())
    ) {
      result.push(allTracksId[i]);
    }
  }
  return result;
}

function fillPickBtn(element: HTMLElement, track: Track, id: Array<number>) {
  const trackName = element.querySelector("h5");
  const artistName = element.querySelector("h6");
  const addBtn = element.querySelector(".add-btn");

  addBtn!.addEventListener("click", (_) => {
    addTrackToQueue(id);
  });

  trackName!.textContent = track.metadata.title;
  artistName!.textContent = track.metadata.artist;
}
