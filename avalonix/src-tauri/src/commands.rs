use std::{
    sync::{Arc, Mutex},
    time::Duration,
};

use avalonix_api::{
    disk::{db::DB, settings::Settings},
    logger,
    media::{media_player::MediaPlayer, play_queue::PlayQueue},
    metadata::{
        album_filter_metadata::AlbumFilterMetadata, track_filter_metadata::TrackFilterMetadata,
        track_metadata::TrackMetadata,
    },
};

#[tauri::command]
pub async fn get_tracks_filter_datas(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
) -> Result<Vec<TrackFilterMetadata>, String> {
    logger::debug("get_track_filter_datas");

    let db = db.lock().unwrap();
    db.get_tracks_filter_metadatas()
        .map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn update_library(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
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
pub async fn next_track(play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>) -> Result<(), String> {
    logger::debug("next_track");
    let mut guard = play_queue.lock().unwrap();
    guard.next().map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn previous_track(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<(), String> {
    logger::debug("previous_track");
    let mut guard = play_queue.lock().unwrap();
    guard.back().map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn pause_or_continue_track(
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) -> Result<bool, String> {
    logger::debug("pause_track");

    let mut guard = media_player.lock().unwrap();

    Ok(guard.pause_or_continue())
}

#[tauri::command]
pub async fn is_paused(
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) -> Result<bool, String> {
    let guard = media_player.lock().unwrap();
    Ok(guard.is_paused())
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
    let library_guard = guard.db.lock().unwrap();
    match library_guard.db_hash.tracks_hash.get(guard.cur_track_index) {
        Some(hash) => {
            return Ok(hash.metadata.clone());
        }
        None => return Err("Inedx out of array tracks_hash".to_string()),
    }
}

#[tauri::command]
pub async fn get_track_cover(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<String, String> {
    let guard = play_queue.lock().unwrap();
    if guard.tracks_in_queue_indexes.is_empty() {
        return Err("Queue is empty".to_string());
    }

    let library_guard = guard.db.lock().unwrap();
    match library_guard.db_hash.tracks_hash.get(guard.cur_track_index) {
        Some(hash) => {
            return hash.get_cover_as_uri().map_err(|err| err.to_string());
        }
        None => {
            return Err("Index out of array tracks_hash".to_string());
        }
    }
}

#[tauri::command]
pub async fn update_tracks_library(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
    settings: tauri::State<'_, Mutex<Settings>>,
) -> Result<(), String> {
    let mut guard = db.lock().unwrap();
    let settings = settings.lock().unwrap();

    let res = guard
        .update_library(&settings)
        .map_err(|err| err.to_string());
    logger::debug("Update library");
    res
}

#[tauri::command]
pub async fn seek(
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
    seek_second: u64,
) -> Result<(), String> {
    let mut guard = media_player.lock().unwrap();
    guard
        .seek(Duration::from_secs(seek_second))
        .map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn get_track_durration(
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) -> Result<u64, String> {
    let mut guard = media_player.lock().unwrap();
    Ok(guard.get_len().as_secs())
}

#[tauri::command]
pub async fn get_track_position(
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) -> Result<u64, String> {
    let mut guard = media_player.lock().unwrap();
    Ok(guard.get_pos().as_secs())
}

#[tauri::command]
pub async fn get_albums_filter_datas(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
) -> Result<Vec<AlbumFilterMetadata>, String> {
    let mut guard = db.lock().unwrap();

    guard
        .get_albums_filter_datas()
        .map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn get_albums_ids(db: tauri::State<'_, Arc<Mutex<DB>>>) -> Result<Vec<Vec<u8>>, String> {
    let guard = db.lock().unwrap();
    guard.get_albums_ids().map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn get_album_cover_by_id(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
    id: Vec<u8>,
) -> Result<String, String> {
    let guard = db.lock().unwrap();

    if let Some(album) = guard
        .db_hash
        .albums_hash
        .iter()
        .find(|x| x.album_metadata.id == id)
    {
        return Ok(album.album_metadata.album_cover.clone());
    }

    Err(format!("album with id: {:?} not found", id))
}

#[tauri::command]
pub async fn add_album_by_id(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    id: Vec<u8>,
) -> Result<(), String> {
    let mut guard = play_queue.lock().unwrap();
    guard.add_album(id).map_err(|err| err.to_string())
}
