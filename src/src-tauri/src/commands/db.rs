use std::sync::{Arc, Mutex, RwLock};

use avalonix_api::{
    disk::{db::DB, user::settings::UserSettings},
    media::{
        self,
        cover_get::CoverGet,
        playable_type::{
            MediaType::{self, Playlist},
            PlayableResult,
        },
        playlist,
    },
};

#[tauri::command]
pub async fn get_playables_ids(
    db: tauri::State<'_, Arc<RwLock<DB>>>,
    media_type: MediaType,
) -> Result<Vec<String>, String> {
    db.read()
        .unwrap()
        .get_uuids(media_type)
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

        MediaType::Playlist => {
            let playlists = guard.get_every_playlist().map_err(|err| err.to_string())?;

            let playlist = playlists
                .into_iter()
                .find(|playlists| playlists.uuid == id)
                .ok_or_else(|| format!("Playlist with id {} not found", id))?;
            PlayableResult::Playlist(playlist)
        }
    };
    Ok(res)
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
