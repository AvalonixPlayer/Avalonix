import type { Track } from "./bindings/Track";
import { invoke } from '@tauri-apps/api/core';

export let allTracks: Track[];
export let allAlbums: { [key in string]: Array<Track> };
export let allArtists: { [key in string]: Array<Track> };

export async function getAllTracks() {
    allTracks = await invoke<Track[]>('get_all_tracks');
    console.log(allTracks);
}

export async function getAllAlbums() {
    allAlbums = await invoke<{ [key in string]: Array<Track> }>('get_all_albums');
    console.log(allAlbums);
}

export async function getAllArtists() {
    allAlbums = await invoke<{ [key in string]: Array<Track> }>('get_all_artists');
    console.log(allAlbums);
}