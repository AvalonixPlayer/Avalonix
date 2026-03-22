use std::{collections::HashMap, sync::Arc};

use avalonix_api::{
    media::track::Track,
    playboxes::playboxes::{self, AlbumsContainer, PlayboxesManager},
};

#[tauri::command]
pub fn get_all_tracks(playboxes: tauri::State<'_, PlayboxesManager>) -> Vec<Arc<Track>> {
    playboxes.tracks_container.all_tracks.clone()
}

#[tauri::command]
pub fn get_all_albums(
    playboxes: tauri::State<'_, PlayboxesManager>,
) -> HashMap<String, Vec<Arc<Track>>> {
    playboxes.albums_container.albums.clone()
}

#[tauri::command]
pub fn get_all_artists(
    playboxes: tauri::State<'_, PlayboxesManager>,
) -> HashMap<String, Vec<Arc<Track>>> {
    playboxes.artists_container.artists.clone()
}
