use std::{
    collections::HashMap,
    sync::{Arc, Mutex},
};

use avalonix_api::{
    audio::media_player::MediaPlayer,
    logger,
    media::track::Track,
    playboxes::{play_queue::PlayQueue, playboxes::PlayboxesManager},
};

#[tauri::command]
pub fn get_all_tracks(playboxes: tauri::State<'_, PlayboxesManager>) -> Vec<Arc<Mutex<Track>>> {
    playboxes.tracks_container.all_tracks.clone()
}

#[tauri::command]
pub fn get_all_albums(
    playboxes: tauri::State<'_, PlayboxesManager>,
) -> HashMap<String, Vec<Arc<Mutex<Track>>>> {
    playboxes.albums_container.albums.clone()
}

#[tauri::command]
pub fn get_all_artists(
    playboxes: tauri::State<'_, PlayboxesManager>,
) -> HashMap<String, Vec<Arc<Mutex<Track>>>> {
    playboxes.artists_container.artists.clone()
}

#[tauri::command]
pub fn add_track_to_queue(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    track: Arc<Mutex<Track>>,
) {
    let mut queue = play_queue.lock().unwrap();
    queue.add_track(track);
}

#[tauri::command]
pub fn remove_track_from_queue(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    track: Arc<Mutex<Track>>,
) {
    let mut queue = play_queue.lock().unwrap();
    queue.remove_track(track);
}

#[tauri::command]
pub fn clear_queue(play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>) {
    let mut queue = play_queue.lock().unwrap();
    queue.clear();
}

#[tauri::command]
pub fn get_queue(play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>) -> Vec<Arc<Mutex<Track>>> {
    let queue = play_queue.lock().unwrap();
    queue.tracks.clone()
}

#[tauri::command]
pub async fn get_len(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<Vec<Arc<Mutex<Track>>>, ()> {
    let queue = play_queue.lock().unwrap();
    Ok(queue.tracks.clone())
}

#[tauri::command]
pub fn pause_or_continue(play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>) {
    let queue = play_queue.lock().unwrap();
    queue.pause_or_continue();
}

#[tauri::command]
pub fn next_track(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) {
    PlayQueue::next_track(&play_queue, &media_player);
}

#[tauri::command]
pub fn previous_track(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) {
    PlayQueue::previous_track(&play_queue, &media_player);
}
