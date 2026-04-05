import type { Track } from "./bindings/Track";
import { invoke } from "@tauri-apps/api/core";
import { addTrackToQueue } from "./playQueue";
import { Album } from "./bindings/Album";

export let allTracks: Track[];
export let allAlbums: { [key in string]: Array<Album> };
export let allArtists: { [key in string]: Array<Track> };

export async function getAllTracks() {
  allTracks = await invoke<Track[]>("get_all_tracks");
  await fillTracksList();
}

export async function getAllAlbums() {
  allAlbums = await invoke<{ [key in string]: Array<Album> }>("get_all_albums");
}

export async function getAllArtists() {
  allArtists =
    await invoke<{ [key in string]: Array<Track> }>("get_all_artists");
}

const container = document.getElementById("tracks-list");

const pickBtnTempl = document.querySelector(
  "#pick-track-btn-example",
) as HTMLTemplateElement;

async function fillTracksList() {
  let searcher = document.getElementById(
    "track-search-area",
  ) as HTMLInputElement;

  searcher.addEventListener("input", async (_) => {
    await fill();
  });
  await fill();

  async function fill() {
    if (!container || !pickBtnTempl) return;

    container.innerHTML = "";
    allTracks.forEach((track) => {
      if (
        searcher!.value != "" &&
        track.metadata.title != null &&
        track.metadata.title
          .toLowerCase()
          .includes(searcher!.value.toLowerCase())
      ) {
        const clone = createTrackBtn(track);
        container.appendChild(clone);
      } else if (searcher!.value == "") {
        const clone = createTrackBtn(track);
        container.appendChild(clone);
      }
    });
  }
}

function createTrackBtn(track: Track): DocumentFragment {
  const clone = pickBtnTempl.content.cloneNode(true) as DocumentFragment;

  const trackNameClone = clone.querySelector("h5");
  const artistNameClone = clone.querySelector("h6");
  const addBtn = clone.querySelector(".add-btn");

  addBtn!.addEventListener("click", (_) => {
    addTrackToQueue(track);
  });

  var title = track.metadata.title || "Unknown Title";
  var artist = track.metadata.artist || "Unknown Artist";

  if (trackNameClone) trackNameClone.textContent = title;
  if (artistNameClone) artistNameClone.textContent = artist;
  return clone;
}
