use std::sync::{Arc, Mutex};

use avalonix_api::{
    disk::{db::DB, settings::Settings},
    logger,
    metadata::{
        album_filter_metadata::AlbumFilterMetadata,
        performer_filter_metadata::PerformerFilterMetadata,
        track_filter_metadata::TrackFilterMetadata,
    },
};
use better_sms::mutex::{MutexGuardWork, MutexWork};

#[tauri::command]
pub async fn get_tracks_filter_datas(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
) -> Result<Vec<TrackFilterMetadata>, String> {
    logger::debug("get_track_filter_datas");

    db.lock_unw().use_guard(|guard| {
        guard
            .get_tracks_filter_metadatas()
            .map_err(|err| err.to_string())
    })
}

#[tauri::command]
pub async fn update_library(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
    settings: tauri::State<'_, Arc<Mutex<Settings>>>,
) -> Result<(), String> {
    logger::debug("update_library");
    db.lock_unw().use_guard(|guard| {
        settings.lock_unw().use_guard(|settings_guard| {
            guard
                .update_library(&settings_guard)
                .map_err(|err| err.to_string())
        })
    })
}

#[tauri::command]
pub async fn get_albums_filter_datas(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
) -> Result<Vec<AlbumFilterMetadata>, String> {
    db.lock_unw().use_guard(|guard| {
        guard
            .get_albums_filter_datas()
            .map_err(|err| err.to_string())
    })
}

#[tauri::command]
pub async fn get_albums_ids(db: tauri::State<'_, Arc<Mutex<DB>>>) -> Result<Vec<Vec<u8>>, String> {
    db.lock_unw()
        .use_guard(|guard| guard.get_albums_ids().map_err(|err| err.to_string()))
}

#[tauri::command]
pub async fn get_album_cover_by_id(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
    id: Vec<u8>,
) -> Result<String, String> {
    db.lock_unw().use_guard(|guard| {
        let albums = guard.get_albums_hash().map_err(|err| err.to_string())?;
        if let Some(album) = albums.iter().find(|x| x.album_metadata.id == id) {
            return Ok(album.album_metadata.album_cover.clone());
        }
        Err(format!("album with id: {:?} not found", id))
    })
}

#[tauri::command]
pub async fn get_performers_filter_datas(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
) -> Result<Vec<PerformerFilterMetadata>, String> {
    db.lock_unw().use_guard(|guard| {
        guard
            .get_performers_filter_datas()
            .map_err(|err| err.to_string())
    })
}

#[tauri::command]
pub async fn get_performers_ids(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
) -> Result<Vec<Vec<u8>>, String> {
    db.lock_unw()
        .use_guard(|guard| guard.get_performers_ids().map_err(|err| err.to_string()))
}

#[tauri::command]
pub async fn clear_library(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
    settings: tauri::State<'_, Arc<Mutex<Settings>>>,
) -> Result<(), String> {
    db.lock_unw().use_guard(|guard| {
        settings.lock_unw().use_guard(|settings_guard| {
            guard
                .clear_library(settings_guard)
                .map_err(|err| err.to_string())
        })
    })
}
