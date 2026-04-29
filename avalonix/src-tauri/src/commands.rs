use std::sync::{Arc, Mutex};

use avalonix_api::{
    disk::{
        db::DB,
        settings::{self, Settings},
    },
    logger,
    media::{media_player, play_queue::PlayQueue},
    metadata::track_filter_metadata::TrackFilterMetadata,
};

#[tauri::command]
pub async fn get_tracks_filter_datas(
    db: tauri::State<'_, Mutex<DB>>,
) -> Result<Vec<TrackFilterMetadata>, String> {
    logger::debug("get_track_filter_datas");

    let db = db.lock().unwrap();
    db.get_tracks_filter_metadatas()
        .map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn get_albums_ids(db: tauri::State<'_, Mutex<DB>>) -> Result<Vec<Vec<u8>>, String> {
    logger::debug("get_albums_ids");

    let db = db.lock().unwrap();
    db.get_albums_ids().map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn update_library(
    db: tauri::State<'_, Mutex<DB>>,
    settings: tauri::State<'_, Mutex<Settings>>,
) -> Result<(), String> {
    logger::debug("update_library");
    let mut db = db.lock().unwrap();
    let settings = settings.lock().unwrap();
    match db.update_library(&settings) {
        Ok(_) => Ok(()),
        Err(err) => Err(err.to_string()),
    }
}

#[tauri::command]
pub async fn start_track(
    media_player: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    index: usize,
) -> Result<(), String> {
    logger::debug("start_track");
    let mut media_player_guard = media_player.lock().unwrap();
    _ = media_player_guard
        .add_track(index)
        .map_err(|err| err.to_string());

    media_player_guard
        .start_track()
        .map_err(|err| err.to_string())
}
