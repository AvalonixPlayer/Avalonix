use std::sync::{Arc, Mutex};

use avalonix_api::{
    disk::db::DB,
    media::playable_type::{MediaType, PlayableResult},
};
use better_sms::mutex::MutexWork;

#[tauri::command]
pub async fn get_playables_ids(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
    media_type: MediaType,
) -> Result<Vec<String>, String> {
    let guard = db.lock_unw();

    let mut ids = vec![];
    match media_type {
        MediaType::Track => {
            for track in guard.get_every_track().map_err(|err| err.to_string())? {
                ids.push(track.uuid);
            }
        }
        MediaType::Album => {
            for album in guard.get_every_album().map_err(|err| err.to_string())? {
                ids.push(album.uuid);
            }
        }
        MediaType::Performer => {
            for performer in guard.get_every_performer().map_err(|err| err.to_string())? {
                ids.push(performer.uuid);
            }
        }
    };
    Ok(ids)
}

#[tauri::command]
pub async fn update_library(db: tauri::State<'_, Arc<Mutex<DB>>>) -> Result<(), String> {
    let guard = db.lock_unw();
    guard.update().map_err(|err| err.to_string())
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
pub async fn get_playable_by_id(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
    media_type: MediaType,
    id: String,
) -> Result<PlayableResult, String> {
    let guard = db.lock_unw();
    let res = match media_type {
        MediaType::Track => {
            let tracks = guard.get_every_track().map_err(|err| err.to_string())?;

            let track = tracks
                .into_iter()
                .find(|performer| performer.uuid == id)
                .ok_or_else(|| format!("Track with id {} not found", id))?;
            PlayableResult::Track(track)
        }
        MediaType::Album => {
            let albums = guard.get_every_album().map_err(|err| err.to_string())?;

            let album = albums
                .into_iter()
                .find(|performer| performer.uuid == id)
                .ok_or_else(|| format!("Album with id {} not found", id))?;
            PlayableResult::Album(album)
        }
        MediaType::Performer => {
            let performers = guard.get_every_performer().map_err(|err| err.to_string())?;

            let performer = performers
                .into_iter()
                .find(|performer| performer.uuid == id)
                .ok_or_else(|| format!("Performer with id {} not found", id))?;
            PlayableResult::Performer(performer)
        }
    };
    Ok(res)
}
