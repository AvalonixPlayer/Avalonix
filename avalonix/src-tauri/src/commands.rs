use std::{
    sync::{Arc, Mutex},
    time::Duration,
};

use avalonix_api::{
    disk::{db::DB, settings::Settings},
    logger,
    media::{media_player::MediaPlayer, play_queue::PlayQueue},
    metadata::{
        album_filter_metadata::AlbumFilterMetadata,
        performer_filter_metadata::PerformerFilterMetadata,
        track_filter_metadata::TrackFilterMetadata, track_metadata::TrackMetadata,
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
    settings: tauri::State<'_, Arc<Mutex<Settings>>>,
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
    id: Vec<u8>,
) -> Result<(), String> {
    logger::debug("start_track");
    let mut play_queue_guard = play_queue.lock().unwrap();
    _ = play_queue_guard
        .add_track(id.clone())
        .map_err(|err| err.to_string());
    play_queue_guard.cur_track_id = id;
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
pub async fn get_tracks_in_queue_ids(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<Vec<Vec<u8>>, String> {
    let guard = play_queue.lock().unwrap();
    let ids = guard.tracks_in_queue_ids.clone();
    Ok(ids)
}

#[tauri::command]
pub async fn get_cur_track_metadata(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<TrackMetadata, String> {
    let play_queue_guard = play_queue.lock().unwrap();
    let library_guard = play_queue_guard.db.lock().unwrap();

    let hash = library_guard
        .get_tracks_hash()
        .map_err(|err| err.to_string());

    match hash {
        Ok(hash) => {
            if let Some(res) = hash
                .iter()
                .find(|x| x.metadata.id == play_queue_guard.cur_track_id)
            {
                return Ok(res.metadata.clone());
            }
            return Err("Inedx out of array tracks_hash".to_string());
        }
        Err(_) => return Err("Inedx out of array tracks_hash".to_string()),
    }
}

#[tauri::command]
pub async fn get_track_cover(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<String, String> {
    let play_queue_guard = play_queue.lock().unwrap();
    let library_guard = play_queue_guard.db.lock().unwrap();

    let hash = library_guard
        .get_tracks_hash()
        .map_err(|err| err.to_string());

    match hash {
        Ok(hash) => {
            if let Some(res) = hash
                .iter()
                .find(|x| x.metadata.id == play_queue_guard.cur_track_id)
            {
                return res.get_cover_as_uri().map_err(|err| err.to_string());
            }
            return Err("Inedx out of array tracks_hash".to_string());
        }
        Err(_) => return Err("Inedx out of array tracks_hash".to_string()),
    }
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
        .get_albums_hash()
        .map_err(|err| err.to_string())?
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
    guard.add_album(id).map_err(|err| err.to_string())?;
    Ok(())
}

#[tauri::command]
pub async fn get_performers_filter_datas(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
) -> Result<Vec<PerformerFilterMetadata>, String> {
    let mut guard = db.lock().unwrap();

    guard
        .get_performers_filter_datas()
        .map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn get_performers_ids(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
) -> Result<Vec<Vec<u8>>, String> {
    let guard = db.lock().unwrap();
    guard.get_performers_ids().map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn add_performer_by_id(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    id: Vec<u8>,
) -> Result<(), String> {
    let mut guard = play_queue.lock().unwrap();
    guard.add_performer(id).map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn get_library_folders_from_settings(
    settings: tauri::State<'_, Arc<Mutex<Settings>>>,
) -> Result<Vec<String>, String> {
    let guard = settings.lock().unwrap();
    let mut res = vec![];

    for i in &guard.lib_paths {
        res.push(i.clone());
    }

    Ok(res)
}

#[tauri::command]
pub async fn add_folder_path_to_library(
    settings: tauri::State<'_, Arc<Mutex<Settings>>>,
    path: String,
) -> Result<(), String> {
    let mut guard = settings.lock().unwrap();
    guard.add_lib_path(path);
    guard.save().map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn remove_folder_path_from_library(
    settings: tauri::State<'_, Arc<Mutex<Settings>>>,
    path: String,
) -> Result<(), String> {
    let mut settings_guard = settings.lock().unwrap();
    settings_guard.remove_path(path);
    settings_guard.save().map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn clear_library(
    db: tauri::State<'_, Arc<Mutex<DB>>>,
    settings: tauri::State<'_, Arc<Mutex<Settings>>>,
) -> Result<(), String> {
    let mut db_guard = db.lock().unwrap();
    let mut settings_guard = settings.lock().unwrap();

    db_guard
        .clear_library(&mut settings_guard)
        .map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn add_track_to_queue(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    id: Vec<u8>,
) -> Result<(), String> {
    let mut guard = play_queue.lock().unwrap();
    guard.add_track(id).map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn play_album_by_id(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    id: Vec<u8>,
) -> Result<(), String> {
    let mut queue_guard = play_queue.lock().unwrap();
    queue_guard.clear_queue().map_err(|err| err.to_string())?;
    queue_guard.add_album(id).map_err(|err| err.to_string())?;
    queue_guard.cur_track_id = queue_guard.tracks_in_queue_ids[0].clone();
    queue_guard.start_track().map_err(|err| err.to_string())?;

    Ok(())
}

#[tauri::command]
pub async fn remove_track_from_queue(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    id: Vec<u8>,
) -> Result<(), String> {
    let mut queue_guard = play_queue.lock().unwrap();
    queue_guard.remove_track(id).map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn get_current_volume(
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) -> Result<f32, String> {
    let media_player_guard = media_player.lock().unwrap();
    Ok(media_player_guard.get_volume())
}

#[tauri::command]
pub async fn set_current_volume(
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
    volume: f32,
) -> Result<(), String> {
    let media_player_guard = media_player.lock().unwrap();
    Ok(media_player_guard.set_volume(volume))
}

#[tauri::command]
pub async fn shuffle_or_unshuffle(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<bool, ()> {
    let mut play_queue_guard = play_queue.lock().unwrap();
    Ok(play_queue_guard.shuffle_or_unshuffle())
}

#[tauri::command]
pub async fn shuffle_state(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<bool, ()> {
    let play_queue_guard = play_queue.lock().unwrap();
    Ok(play_queue_guard.shuffle)
}
