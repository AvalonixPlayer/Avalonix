use std::{
    collections::{BTreeMap, HashMap},
    sync::{Arc, Mutex, RwLock},
};

use avalonix_api::{
    audio::media_player::MediaPlayer,
    disk::{db::DB, sort_by::SortBy, user::settings::UserSettings},
    logger::fatal,
    media::{
        self,
        cover_get::CoverGet,
        play_queue::PlayQueue,
        playable_type::{
            MediaType::{self, Playlist},
            PlayableResult,
        },
        playlist,
    },
};
use better_sms::mutex::MutexWork;

#[tauri::command]
pub async fn get_playables_ids(
    db: tauri::State<'_, Arc<RwLock<DB>>>,
    media_type: MediaType,
    sort_by: SortBy,
) -> Result<BTreeMap<String, Vec<String>>, String> {
    db.read()
        .unwrap()
        .get_uuids(media_type, sort_by)
        .map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn get_playables_ids_by_part_of_name(
    db: tauri::State<'_, Arc<RwLock<DB>>>,
    part_of_name: String,
    media_type: MediaType,
) -> Result<Vec<String>, String> {
    db.read()
        .unwrap()
        .get_ids_by_part_of_name(&part_of_name, media_type)
        .map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn update_library(
    db: tauri::State<'_, Arc<RwLock<DB>>>,
    settings: tauri::State<'_, Arc<Mutex<UserSettings>>>,
) -> Result<(), String> {
    let guard = db.read().unwrap();
    guard
        .update(&mut settings.lock().unwrap())
        .map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn get_album_performer_name_by_id(
    db: tauri::State<'_, Arc<RwLock<DB>>>,
    id: String,
) -> Result<String, String> {
    let guard = db.read().unwrap();
    let albums = guard.get_every_album().map_err(|err| err.to_string())?;

    Ok(albums
        .into_iter()
        .find(|album| album.uuid == id)
        .ok_or_else(|| format!("Album with id {} not found", id))?
        .performer)
}

#[tauri::command]
pub async fn get_playable_by_id(
    db: tauri::State<'_, Arc<RwLock<DB>>>,
    media_type: MediaType,
    id: String,
) -> Result<PlayableResult, String> {
    let guard = db.read().unwrap();
    guard
        .get_media_by_id(id, media_type)
        .map_err(|err| err.to_string())
}

#[tauri::command]
pub async fn get_track_cover(
    db: tauri::State<'_, Arc<RwLock<DB>>>,
    id: String,
) -> Result<String, String> {
    let playable = get_playable_by_id(db, MediaType::Track, id).await?;

    if let PlayableResult::Track(track) = playable {
        Ok(track.get_cover_as_uri())
    } else {
        Err("Expected a track, but got something else".to_string())
    }
}

#[tauri::command]
pub async fn create_playlist(
    db: tauri::State<'_, Arc<RwLock<DB>>>,
    playlist_name: String,
) -> Result<(), String> {
    let db_guard = &db.write().unwrap();
    media::playlist::Playlist::create_new_playlist(db_guard, playlist_name)
        .map_err(|err| err.to_string())?;
    Ok(())
}

#[tauri::command]
pub async fn remove_playlist(
    db: tauri::State<'_, Arc<RwLock<DB>>>,
    playlist_uuid: String,
) -> Result<(), String> {
    let db_guard = &db.write().unwrap();
    db_guard
        .remove_playlist(playlist_uuid)
        .map_err(|err| err.to_string())?;
    Ok(())
}

#[tauri::command]
pub async fn add_track_to_playlist(
    db: tauri::State<'_, Arc<RwLock<DB>>>,
    playlist_uuid: String,
    track_uuid: String,
) -> Result<(), String> {
    let db_guard = &db.write().unwrap();
    db_guard
        .add_track_to_playlist(playlist_uuid, track_uuid)
        .map_err(|err| err.to_string())?;
    Ok(())
}

#[tauri::command]
pub async fn edit_track(
    db: tauri::State<'_, Arc<RwLock<DB>>>,
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    uuid: String,
    title: String,
    album: String,
    performer: String,
    genre: String,
    path_to_cover: Option<String>,
) -> Result<(), String> {
    let mut queue = play_queue.lock_unw();

    if let Some(cur_uuid) = &queue.current_uuid {
        if *cur_uuid == uuid {
            queue
                .remove_track(uuid.clone())
                .map_err(|err| err.to_string())?;
        }
    }

    let track = db
        .read()
        .unwrap()
        .get_media_by_id(uuid.clone(), MediaType::Track)
        .unwrap()
        .unwrap_as_track();

    db.write()
        .unwrap()
        .edit_track(track, uuid, title, album, performer, genre, path_to_cover)
        .map_err(|err| err.to_string())?;

    Ok(())
}

#[tauri::command]
pub async fn edit_album(
    db: tauri::State<'_, Arc<RwLock<DB>>>,
    settings: tauri::State<'_, Arc<Mutex<UserSettings>>>,
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    uuid: String,
    title: String,
    performer: String,
) -> Result<(), String> {
    db.write()
        .unwrap()
        .edit_album(uuid, title, performer)
        .map_err(|err| err.to_string())?;
    Ok(())
}
#[tauri::command]
pub async fn edit_performer(
    db: tauri::State<'_, Arc<RwLock<DB>>>,
    settings: tauri::State<'_, Arc<Mutex<UserSettings>>>,
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    uuid: String,
    title: String,
) -> Result<(), String> {
    db.write()
        .unwrap()
        .edit_performer(uuid, title)
        .map_err(|err| err.to_string())?;
    Ok(())
}
