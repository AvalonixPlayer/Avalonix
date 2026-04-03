use std::{
    collections::HashMap,
    sync::{mpsc::Sender, Arc, Mutex},
};

use avalonix_api::{
    audio::media_player::MediaPlayer,
    media::track::Track,
    playboxes::{
        album::Album,
        play_queue::{PlayQueue, PlayQueueAction},
        playboxes::PlayboxesManager,
    },
};

#[tauri::command]
pub fn get_all_tracks(playboxes: tauri::State<'_, PlayboxesManager>) -> Vec<Arc<Mutex<Track>>> {
    playboxes.tracks_container.all_tracks.clone()
}

#[tauri::command]
pub fn get_all_albums(
    playboxes: tauri::State<'_, PlayboxesManager>,
) -> HashMap<String, Arc<Mutex<Album>>> {
    let albums = playboxes.albums_container.albums.clone();
    albums
}

#[tauri::command]
pub fn get_all_artists(
    playboxes: tauri::State<'_, PlayboxesManager>,
) -> HashMap<String, Vec<Arc<Mutex<Track>>>> {
    playboxes.artists_container.artists.clone()
}

#[tauri::command]
pub fn add_track_to_queue(
    play_queue_action_sender: tauri::State<'_, Arc<Mutex<Sender<PlayQueueAction>>>>,
    track: Arc<Mutex<Track>>,
) {
    let sender = play_queue_action_sender.lock().unwrap();
    sender.send(PlayQueueAction::AddTrack(track)).unwrap();
}

#[tauri::command]
pub fn remove_track_from_queue(
    play_queue_action_sender: tauri::State<'_, Arc<Mutex<Sender<PlayQueueAction>>>>,
    trackPath: String,
) {
    let sender = play_queue_action_sender.lock().unwrap();
    sender
        .send(PlayQueueAction::RemoveTrack(trackPath))
        .unwrap();
}

#[tauri::command]
pub fn clear_queue(
    play_queue_action_sender: tauri::State<'_, Arc<Mutex<Sender<PlayQueueAction>>>>,
) {
    let sender = play_queue_action_sender.lock().unwrap();
    sender.send(PlayQueueAction::Clear).unwrap();
}

#[tauri::command]
pub fn get_queue(play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>) -> Vec<Arc<Mutex<Track>>> {
    let queue = play_queue.lock().unwrap();
    let clone = queue.tracks.clone();
    clone
}

#[tauri::command]
pub async fn get_len(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
) -> Result<Vec<Arc<Mutex<Track>>>, ()> {
    let queue = play_queue.lock().unwrap();
    Ok(queue.tracks.clone())
}

#[tauri::command]
pub fn pause_or_continue(
    play_queue_action_sender: tauri::State<'_, Arc<Mutex<Sender<PlayQueueAction>>>>,
) {
    let sender = play_queue_action_sender.lock().unwrap();
    sender.send(PlayQueueAction::PauseOrContinue).unwrap();
}

#[tauri::command]
pub fn next_track(play_queue_action_sender: tauri::State<'_, Arc<Mutex<Sender<PlayQueueAction>>>>) {
    let sender = play_queue_action_sender.lock().unwrap();
    sender.send(PlayQueueAction::Next).unwrap();
}

#[tauri::command]
pub fn previous_track(
    play_queue_action_sender: tauri::State<'_, Arc<Mutex<Sender<PlayQueueAction>>>>,
) {
    let sender = play_queue_action_sender.lock().unwrap();
    sender.send(PlayQueueAction::Previous).unwrap();
}

#[tauri::command]
pub fn play_track(
    play_queue_action_sender: tauri::State<'_, Arc<Mutex<Sender<PlayQueueAction>>>>,
    track: Arc<Mutex<Track>>,
) {
    let sender = play_queue_action_sender.lock().unwrap();
    sender.send(PlayQueueAction::Switch(track)).unwrap();
}

#[tauri::command]
pub fn on_pause(media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>) -> bool {
    let guard = media_player.lock().unwrap();
    guard.is_paused()
}
