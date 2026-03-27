use std::{
    collections::HashMap,
    sync::{Arc, Mutex},
};

use avalonix_api::{
    media::track::Track,
    playboxes::{play_queue::PlayQueue, playboxes::PlayboxesManager},
};

#[tauri::command]
pub fn get_all_tracks(playboxes: tauri::State<'_, PlayboxesManager>) -> Vec<Arc<Track>> {
    playboxes.tracks_container.all_tracks.clone()
}

#[tauri::command]
pub fn get_all_albums(
    playboxes: tauri::State<'_, PlayboxesManager>,
) -> HashMap<String, Vec<Arc<Track>>> {
    playboxes.albums_container.albums.clone()
}

#[tauri::command]
pub fn get_all_artists(
    playboxes: tauri::State<'_, PlayboxesManager>,
) -> HashMap<String, Vec<Arc<Track>>> {
    playboxes.artists_container.artists.clone()
}

#[tauri::command]
pub fn add_track_to_queue(play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>, track: Arc<Track>) {
    loop {
        let queue = play_queue.try_lock();
        match queue {
            Ok(mut queue) => {
                queue.add_track(track);
                break;
            }
            Err(_) => {}
        }
    }
}

#[tauri::command]
pub fn remove_track_from_queue(
    play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>,
    track: Arc<Track>,
) {
    loop {
        let queue = play_queue.try_lock();
        match queue {
            Ok(mut queue) => {
                queue.remove_track(track);
                break;
            }
            Err(_) => {}
        }
    }
}

#[tauri::command]
pub fn clear_queue(play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>) {
    loop {
        let queue = play_queue.try_lock();
        match queue {
            Ok(mut queue) => {
                queue.clear();
                break;
            }
            Err(_) => {}
        }
    }
}

#[tauri::command]
pub fn get_queue(play_queue: tauri::State<'_, Arc<Mutex<PlayQueue>>>) -> Vec<Arc<Track>> {
    loop {
        let queue = play_queue.try_lock();
        match queue {
            Ok(queue) => return queue.tracks.clone(),
            Err(_) => {}
        }
    }
}
