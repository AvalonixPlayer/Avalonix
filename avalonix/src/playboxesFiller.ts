import type { Track } from "./bindings/Track";
import { invoke } from '@tauri-apps/api/core';

export let allTracks: Track[];
export let allAlbums: { [key in string]: Array<Track> };
export let allArtists: { [key in string]: Array<Track> };

export async function getAllTracks() {
    allTracks = await invoke<Track[]>('get_all_tracks');
    await appendTracksList();
}

export async function getAllAlbums() {
    allAlbums = await invoke<{ [key in string]: Array<Track> }>('get_all_albums');
    console.log(allAlbums);
}

export async function getAllArtists() {
    allAlbums = await invoke<{ [key in string]: Array<Track> }>('get_all_artists');
    console.log(allAlbums);
}

export let albumsElements = await document.createElement("div");



var pickBtnTempl = document.querySelector('#pick-track-btn-example') as HTMLTemplateElement;

async function appendTracksList() {
    const container = document.getElementById("tracks-list");
    if (!container || !pickBtnTempl) return;

    container.innerHTML = "";
    let tracksIndex = 0;
    allTracks.forEach(track => {
        tracksIndex++;
        var name = track.metadata.title || "Unknown Title";
        name = tracksIndex + ". " + name;

        if(name.length > 35)
        {
            name = name.slice(0,35);
            name = name + "...";
        }

        const clone = createTrackBtn(name);
        container.appendChild(clone);
    });
}

function createTrackBtn(trackName: string): DocumentFragment {
    const clone = pickBtnTempl.content.cloneNode(true) as DocumentFragment;
    const h3 = clone.querySelector("h5");
    if (h3) h3.textContent = trackName;
    return clone;
}