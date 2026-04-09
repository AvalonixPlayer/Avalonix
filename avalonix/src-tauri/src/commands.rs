use std::sync::{mpsc::Sender, Arc, Mutex};

use avalonix_api::{
    audio::media_player::MediaPlayer,
    db::MusicDB,
    logger,
    playable::{
        filter_data::FilterData,
        play_queue::{PlayQueue, PlayQueueAction},
        playboxes::PlayboxesManager,
        track::{self, Track},
    },
    settings_manager::Settings,
};

#[tauri::command]
pub fn get_all_tracks_id(playboxes: tauri::State<'_, PlayboxesManager>) -> Vec<Vec<u8>> {
    playboxes.tracks_container.all_tracks_id.clone()
}

#[tauri::command]
pub async fn get_track_by_id(
    playboxes: tauri::State<'_, PlayboxesManager>,
    db: tauri::State<'_, MusicDB>,
    id: Vec<u8>,
) -> Result<Track, ()> {
    playboxes.tracks_container.get_track_by_id(&db, id)
}

#[tauri::command]
pub async fn get_all_tracks_filter_data(
    db: tauri::State<'_, MusicDB>,
) -> Result<Vec<FilterData>, ()> {
    db.get_all_tracks_filter_data().map_err(|_| ())
}

/*
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
 */

#[tauri::command]
pub fn add_track_to_queue(
    play_queue_action_sender: tauri::State<'_, Arc<Mutex<Sender<PlayQueueAction>>>>,
    playboxes: tauri::State<'_, PlayboxesManager>,
    db: tauri::State<'_, MusicDB>,
    id: Vec<u8>,
) {
    let track = playboxes.tracks_container.get_track_by_id(&db, id);
    match track {
        Ok(track) => {
            let sender = play_queue_action_sender.lock().unwrap();
            sender
                .send(PlayQueueAction::AddTrack(Arc::new(Mutex::new(track))))
                .unwrap();
        }
        Err(_) => {}
    }
}

#[tauri::command]
pub fn remove_track_from_queue(
    play_queue_action_sender: tauri::State<'_, Arc<Mutex<Sender<PlayQueueAction>>>>,
    id: Vec<u8>,
) {
    let sender = play_queue_action_sender.lock().unwrap();
    sender.send(PlayQueueAction::RemoveTrack(id)).unwrap();
}

#[tauri::command]
pub fn clear_queue(
    play_queue_action_sender: tauri::State<'_, Arc<Mutex<Sender<PlayQueueAction>>>>,
) {
    let sender = play_queue_action_sender.lock().unwrap();
    sender.send(PlayQueueAction::Clear).unwrap();
}

#[tauri::command]
pub fn get_queue(play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>) -> Vec<Vec<u8>> {
    let queue = play_queue.lock().unwrap();
    let mut result = Vec::new();
    for i in &queue.tracks {
        let guard = i.lock().unwrap();
        result.push(guard.id.clone().into_bytes());
        drop(guard);
    }
    result
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
    playboxes: tauri::State<'_, PlayboxesManager>,
    db: tauri::State<'_, MusicDB>,
    id: Vec<u8>,
) {
    let track = playboxes.tracks_container.get_track_by_id(&db, id);
    match track {
        Ok(track) => {
            let sender = play_queue_action_sender.lock().unwrap();
            sender
                .send(PlayQueueAction::Switch(Arc::new(Mutex::new(track))))
                .unwrap();
        }
        Err(_) => {}
    }
}

#[tauri::command]
pub fn on_pause(media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>) -> bool {
    let guard = media_player.lock().unwrap();
    guard.is_paused()
}

#[tauri::command]
pub async fn add_music_folder(
    settings: tauri::State<'_, Arc<Mutex<Settings>>>,
    mut paths: Vec<String>,
) -> Result<(), ()> {
    let mut settings_guard = settings.lock().unwrap();
    settings_guard.library_paths.append(&mut paths);
    drop(settings_guard);
    Ok(())
}

#[tauri::command]
pub async fn save_settings(settings: tauri::State<'_, Arc<Mutex<Settings>>>) -> Result<(), ()> {
    let settings_guard = settings.lock().unwrap();
    _ = settings_guard.save_settings();
    drop(settings_guard);
    Ok(())
}
