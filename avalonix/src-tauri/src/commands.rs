use std::sync::{Arc, Mutex};

use avalonix_api::{
    disk::{
        db::DB,
        settings::{self, Settings},
    },
    logger,
    media::{media_player, play_queue::PlayQueue},
    metadata::{track_filter_metadata::TrackFilterMetadata, track_metadata::TrackMetadata},
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
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    index: usize,
) -> Result<(), String> {
    logger::debug("start_track");
    let mut play_queue_guard = play_queue.lock().unwrap();
    _ = play_queue_guard
        .add_track(index)
        .map_err(|err| err.to_string());

    play_queue_guard.cur_track_index = index;
    play_queue_guard
        .start_track()
        .map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn get_tracks_in_queue_indexes(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<Vec<usize>, String> {
    let guard = play_queue.lock().unwrap();

    let indexes = guard.tracks_in_queue_indexes.clone();
    Ok(indexes)
}

#[tauri::command]
pub async fn get_cur_track_metadata(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<TrackMetadata, String> {
    let guard = play_queue.lock().unwrap();
    if guard.tracks_in_queue_indexes.is_empty() {
        return Err("Queue is empty".to_string());
    }
    let metadata = guard.library.tracks_hash[guard.cur_track_index]
        .metadata
        .clone();
    Ok(metadata)
}
