use std::sync::{Arc, Mutex, RwLock};

use avalonix_api::{
    disk::db::DB,
    media::{
        album,
        media_trait::Media,
        play_queue::{self, PlayQueue},
        playable_type::MediaType,
    },
};
use better_sms::mutex::{MutexGuardWork, MutexWork};

#[tauri::command]
pub async fn clear_queue(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<(), String> {
    play_queue.lock_unw().use_guard(|guard| guard.clear());
    Ok(())
}

#[tauri::command]
pub async fn add_media_to_queue(
    db: tauri::State<'_, Arc<RwLock<DB>>>,
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    media_type: MediaType,
    id: String,
) -> Result<(), String> {
    let uuids = match media_type {
        MediaType::Track => vec![id],
        MediaType::Album => {
            let albums = db
                .read()
                .unwrap()
                .get_every_album()
                .map_err(|_| "Error while getting albums from db".to_string())?;

            let album = albums
                .iter()
                .find(|album| album.uuid == id)
                .ok_or_else(|| "Album not found".to_string())?;

            album.get_tracks_uuids().clone()
        }
        MediaType::Performer => {
            let performers = db
                .read()
                .unwrap()
                .get_every_performer()
                .map_err(|_| "Error while getting performers from db".to_string())?;

            let performer = performers
                .iter()
                .find(|performer| performer.uuid == id)
                .ok_or_else(|| "Performer not found".to_string())?;

            performer.get_tracks_uuids().clone()
        }
    };
    play_queue
        .lock_unw()
        .add_tracks(uuids)
        .map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn get_queue_tracks_ids(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<Vec<String>, String> {
    Ok(play_queue
        .lock_unw()
        .tracks_uuids_in_queue_displaying
        .clone())
}

#[tauri::command]
pub async fn next_track(play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>) -> Result<(), String> {
    play_queue
        .lock_unw()
        .use_guard(|guard| guard.next())
        .map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn previous_track(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<(), String> {
    play_queue
        .lock_unw()
        .use_guard(|guard| guard.back())
        .map_err(|err| err.to_string())
}
