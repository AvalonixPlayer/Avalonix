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

var pickBtnTempl = document.querySelector('#pick-track-btn-example') as HTMLTemplateElement;

async function appendTracksList() {
    const container = document.getElementById("tracks-list");
    if (!container || !pickBtnTempl) return;

    container.innerHTML = "";
    let tracksIndex = 0;
    allTracks.forEach(track => {
        tracksIndex++;

        const clone = createTrackBtn(track, tracksIndex);
        container.appendChild(clone);
    });
}

function createTrackBtn(track: Track, index: number): DocumentFragment {
    const clone = pickBtnTempl.content.cloneNode(true) as DocumentFragment;
    
    const trackNameClone = clone.querySelector("h5");
    const artistNameClone = clone.querySelector("h6");
    
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