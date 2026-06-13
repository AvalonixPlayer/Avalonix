use std::sync::{Arc, Mutex};

use avalonix_api::{
    disk::db::DB,
    media::{album::Album, performer::Performer, track::Track},
};
use better_sms::mutex::MutexWork;

#[tauri::command]
pub async fn get_tracks_datas(db: tauri::State<'_, Arc<Mutex<DB>>>) -> Result<Vec<Track>, String> {
    let guard = db.lock_unw();
    guard.get_every_track().map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn get_albums_datas(db: tauri::State<'_, Arc<Mutex<DB>>>) -> Result<Vec<Album>, String> {
    let guard = db.lock_unw();
    guard.get_every_album().map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn get_performers_datas(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
) -> Result<Vec<Performer>, String> {
    let guard = db.lock_unw();
    guard.get_every_performer().map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn update_library(db: tauri::State<'_, Arc<Mutex<DB>>>) -> Result<(), String> {
    let guard = db.lock_unw();
    guard.update().map_err(|err| err.to_string())
}
