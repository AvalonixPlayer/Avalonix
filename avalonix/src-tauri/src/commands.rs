use std::sync::Arc;

use avalonix_api::{media::track::Track, playboxes::playboxes::PlayboxesManager};

#[tauri::command]
pub fn get_all_tracks(playboxes: tauri::State<'_, PlayboxesManager>) -> Vec<Arc<Track>> {
    playboxes.tracks_container.all_tracks.clone()
}
