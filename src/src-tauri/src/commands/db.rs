use std::sync::{Arc, Mutex};

use avalonix_api::{
    disk::db::DB,
    logger::debug,
    media::{album::Album, performer::Performer, track::Track},
};
use better_sms::mutex::MutexWork;

#[tauri::command]
pub async fn get_tracks_ids(db: tauri::State<'_, Arc<Mutex<DB>>>) -> Result<Vec<String>, String> {
    let guard = db.lock_unw();
    let mut ids = vec![];
    for track in guard.get_every_track().map_err(|err| err.to_string())? {
        ids.push(track.uuid);
    }
    Ok(ids)
}

#[tauri::command]
pub async fn get_albums_ids(db: tauri::State<'_, Arc<Mutex<DB>>>) -> Result<Vec<String>, String> {
    let guard = db.lock_unw();
    let mut ids = vec![];
    for album in guard.get_every_album().map_err(|err| err.to_string())? {
        ids.push(album.uuid);
    }
    Ok(ids)
}

#[tauri::command]
pub async fn get_performers_ids(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
) -> Result<Vec<String>, String> {
    let guard = db.lock_unw();
    let mut ids = vec![];
    for performer in guard.get_every_performer().map_err(|err| err.to_string())? {
        ids.push(performer.uuid);
    }
    Ok(ids)
}

#[tauri::command]
pub async fn update_library(db: tauri::State<'_, Arc<Mutex<DB>>>) -> Result<(), String> {
    let guard = db.lock_unw();
    guard.update().map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn get_track_by_id(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
    id: String,
) -> Result<Track, String> {
    let guard = db.lock_unw();
    let tracks = guard.get_every_track().map_err(|err| err.to_string())?;

    tracks
        .into_iter()
        .find(|track| track.uuid == id)
        .ok_or_else(|| format!("Track with id {} not found", id))
}

#[tauri::command]
pub async fn get_album_by_id(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
    id: String,
) -> Result<Album, String> {
    let guard = db.lock_unw();
    let albums = guard.get_every_album().map_err(|err| err.to_string())?;

    albums
        .into_iter()
        .find(|album| album.uuid == id)
        .ok_or_else(|| format!("Album with id {} not found", id))
}

#[tauri::command]
pub async fn get_album_performer_name_by_id(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
    id: String,
) -> Result<String, String> {
    let guard = db.lock_unw();
    let albums = guard.get_every_album().map_err(|err| err.to_string())?;

    Ok(albums
        .into_iter()
        .find(|album| album.uuid == id)
        .ok_or_else(|| format!("Album with id {} not found", id))?
        .performer)
}

#[tauri::command]
pub async fn get_performer_by_id(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
    id: String,
) -> Result<Performer, String> {
    let guard = db.lock_unw();
    let performers = guard.get_every_performer().map_err(|err| err.to_string())?;

    performers
        .into_iter()
        .find(|performer| performer.uuid == id)
        .ok_or_else(|| format!("Performer with id {} not found", id))
}
