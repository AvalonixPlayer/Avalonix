import type { Track } from "./bindings/Track";
import { invoke } from "@tauri-apps/api/core";
import { addTrackToQueue } from "./playQueue";

export let allTracks: Track[];
export let allAlbums: { [key in string]: Array<Track> };
export let allArtists: { [key in string]: Array<Track> };

export async function getAllTracks() {
  allTracks = await invoke<Track[]>("get_all_tracks");
  await appendTracksList();
}

export async function getAllAlbums() {
  allAlbums = await invoke<{ [key in string]: Array<Track> }>("get_all_albums");
}

export async function getAllArtists() {
  allAlbums =
    await invoke<{ [key in string]: Array<Track> }>("get_all_artists");
}

const pickBtnTempl = document.querySelector(
  "#pick-track-btn-example",
) as HTMLTemplateElement;

async function appendTracksList() {
  let searcher = document.getElementById(
    "track-search-area",
  ) as HTMLInputElement;

  searcher.addEventListener("input", async (_) => {
    await fill();
  });
  await fill();

  async function fill() {
    const container = document.getElementById("tracks-list");
    if (!container || !pickBtnTempl) return;

    container.innerHTML = "";
    let index = 0;
    allTracks.forEach((track) => {
      if (
        searcher!.value != "" &&
        track.metadata.title != null &&
        track.metadata.title
          .toLowerCase()
          .includes(searcher!.value.toLowerCase())
      ) {
        index++;
        const clone = createTrackBtn(track, index);
        container.appendChild(clone);
      } else if (searcher!.value == "") {
        index++;
        const clone = createTrackBtn(track, index);
        container.appendChild(clone);
      }
    });
  }
}

function createTrackBtn(track: Track, index: number): DocumentFragment {
  const clone = pickBtnTempl.content.cloneNode(true) as DocumentFragment;

  const trackNameClone = clone.querySelector("h5");
  const artistNameClone = clone.querySelector("h6");
  const addBtn = clone.querySelector(".add-btn");

  addBtn!.addEventListener("click", (_) => {
    addTrackToQueue(track);
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
