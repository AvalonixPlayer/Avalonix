use std::sync::{Arc, Mutex};

use avalonix_api::{logger, media::play_queue::PlayQueue, metadata::track_metadata::TrackMetadata};
use better_sms::mutex::{MutexGuardWork, MutexWork};

#[tauri::command]
pub async fn start_track(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    id: Vec<u8>,
) -> Result<(), String> {
    logger::debug("start_track");
    play_queue.lock_unw().use_guard(|play_queue_guard| {
        _ = play_queue_guard
            .add_track(id.clone())
            .map_err(|err| err.to_string());
        play_queue_guard.cur_track_id = id;
        play_queue_guard
            .start_track()
            .map_err(|err| err.to_string())
    })
}

#[tauri::command]
pub async fn next_track(play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>) -> Result<(), String> {
    logger::debug("next_track");
    play_queue
        .lock_unw()
        .use_guard(|guard| guard.next().map_err(|err| err.to_string()))
}

#[tauri::command]
pub async fn previous_track(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<(), String> {
    logger::debug("previous_track");
    play_queue
        .lock_unw()
        .use_guard(|guard| guard.back().map_err(|err| err.to_string()))
}

#[tauri::command]
pub async fn get_tracks_in_queue_ids(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<Vec<Vec<u8>>, String> {
    play_queue
        .lock_unw()
        .use_guard(|guard| Ok(guard.tracks_in_queue_ids.clone()))
}

#[tauri::command]
pub async fn get_cur_track_metadata(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<TrackMetadata, String> {
    play_queue.lock_unw().use_guard(|play_queue_guard| {
        play_queue_guard.db.lock_unw().use_guard(|library_guard| {
            let hash = library_guard
                .get_tracks_hash()
                .map_err(|err| err.to_string())?;

            if let Some(res) = hash
                .iter()
                .find(|x| x.metadata.id == play_queue_guard.cur_track_id)
            {
                return Ok(res.metadata.clone());
            }
            Err("Inedx out of array tracks_hash".to_string())
        })
    })
}

#[tauri::command]
pub async fn get_track_cover(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<String, String> {
    play_queue.lock_unw().use_guard(|play_queue_guard| {
        play_queue_guard.db.lock_unw().use_guard(|library_guard| {
            let hash = library_guard
                .get_tracks_hash()
                .map_err(|err| err.to_string())?;

            if let Some(res) = hash
                .iter()
                .find(|x| x.metadata.id == play_queue_guard.cur_track_id)
            {
                return res.get_cover_as_uri().map_err(|err| err.to_string());
            }
            Err("Inedx out of array tracks_hash".to_string())
        })
    })
}

#[tauri::command]
pub async fn add_album_by_id(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    id: Vec<u8>,
) -> Result<(), String> {
    play_queue
        .lock_unw()
        .use_guard(|guard| guard.add_album(id).map_err(|err| err.to_string()))
}

#[tauri::command]
pub async fn add_performer_by_id(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    id: Vec<u8>,
) -> Result<(), String> {
    play_queue
        .lock_unw()
        .use_guard(|guard| guard.add_performer(id).map_err(|err| err.to_string()))
}

#[tauri::command]
pub async fn add_track_to_queue(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    id: Vec<u8>,
) -> Result<(), String> {
    play_queue
        .lock_unw()
        .use_guard(|guard| guard.add_track(id).map_err(|err| err.to_string()))
}

#[tauri::command]
pub async fn play_album_by_id(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    id: Vec<u8>,
) -> Result<(), String> {
    play_queue.lock_unw().use_guard(|queue_guard| {
        queue_guard.clear_queue().map_err(|err| err.to_string())?;
        queue_guard.add_album(id).map_err(|err| err.to_string())?;
        queue_guard.cur_track_id = queue_guard.tracks_in_queue_ids[0].clone();
        queue_guard.start_track().map_err(|err| err.to_string())
    })
}

#[tauri::command]
pub async fn remove_track_from_queue(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    id: Vec<u8>,
) -> Result<(), String> {
    play_queue
        .lock_unw()
        .use_guard(|queue_guard| queue_guard.remove_track(id).map_err(|err| err.to_string()))
}

#[tauri::command]
pub async fn shuffle_or_unshuffle(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<bool, ()> {
    play_queue
        .lock_unw()
        .use_guard(|play_queue_guard| Ok(play_queue_guard.shuffle_or_unshuffle()))
}

#[tauri::command]
pub async fn shuffle_state(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<bool, ()> {
    play_queue
        .lock_unw()
        .use_guard(|play_queue_guard| Ok(play_queue_guard.shuffle))
}
