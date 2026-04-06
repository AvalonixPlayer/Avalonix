import type { Track } from "./bindings/Track";
import { invoke } from "@tauri-apps/api/core";
import { Album } from "./bindings/Album";
import { addTrackToQueue } from "./playQueue";

let allTracksId: Array<Array<number>>;
let allAlbums: { [key in string]: Array<Album> };
let allArtists: { [key in string]: Array<Track> };

export async function getAllTracksId() {
  allTracksId = await invoke<Array<Array<number>>>("get_all_tracks_id");
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
  if (!container || !pickBtnTempl) return;

  container.innerHTML = "";

  allTracksId.forEach((id) => {
    const fragment = pickBtnTempl.content.cloneNode(true) as DocumentFragment;

    const element = fragment.firstElementChild as HTMLElement;

    if (element) {
      const options = {
        root: container,
        threshold: 0.1,
      };

      const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            invoke<Track>("get_track_by_id", { id: id }).then((track) =>
              fillPickBtn(element, track),
            );
          }
          if (!entry.isIntersecting) {
          }
        });
      }, options);

      container.append(fragment);
      observer.observe(element);
    }
  });
}

function fillPickBtn(element: HTMLElement, track: Track) {
  const trackName = element.querySelector("h5");
  const artistName = element.querySelector("h6");

  const addBtn = element.querySelector(".add-btn");

  addBtn!.addEventListener("click", (_) => {
    addTrackToQueue(track);
  });

  trackName!.textContent = track.metadata.title;
  artistName!.textContent = track.metadata.artist;
}

/*function createTrackBtn(track: Track): DocumentFragment {
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
}*/
