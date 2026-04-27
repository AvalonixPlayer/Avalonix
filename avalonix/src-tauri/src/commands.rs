use std::sync::Mutex;

use avalonix_api::{
    disk::{
        db::DB,
        settings::{self, Settings},
    },
    logger,
};

#[tauri::command]
pub async fn get_tracks_ids(db: tauri::State<'_, Mutex<DB>>) -> Result<Vec<Vec<u8>>, String> {
    logger::debug("get_tracks_ids");

    let db = db.lock().unwrap();
    db.get_tracks_ids().map_err(|err| err.to_string())
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
