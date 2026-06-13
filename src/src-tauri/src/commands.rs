use std::sync::{Arc, Mutex};

use avalonix_api::{disk::db::DB, media::track::Track};
use better_sms::mutex::{MutexGuardWork, MutexWork};
use tauri::command;

#[tauri::command]
pub async fn get_tracks_datas(db: tauri::State<'_, Arc<Mutex<DB>>>) -> Result<Vec<Track>, String> {
    let mut result = vec![];
    let guard = db.lock_unw();
    for track in guard.get_every_track().map_err(|err| err.to_string())? {
        result.push(track);
    }

    Ok(result)
}
