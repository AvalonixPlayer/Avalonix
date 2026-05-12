use std::{
    sync::{Arc, Mutex},
    time::Duration,
};

use avalonix_api::{logger, media::media_player::MediaPlayer};
use better_sms::mutex::{MutexGuardWork, MutexWork};

#[tauri::command]
pub async fn pause_or_continue_track(
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) -> Result<bool, String> {
    logger::debug("pause_track");

    media_player
        .lock_unw()
        .use_guard(|guard| Ok(guard.pause_or_continue()))
}

#[tauri::command]
pub async fn is_paused(
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) -> Result<bool, String> {
    media_player
        .lock_unw()
        .use_guard(|guard| Ok(guard.is_paused()))
}

#[tauri::command]
pub async fn seek(
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
    seek_second: u64,
) -> Result<(), String> {
    media_player.lock_unw().use_guard(|guard| {
        guard
            .seek(Duration::from_secs(seek_second))
            .map_err(|err| err.to_string())
    })
}

#[tauri::command]
pub async fn get_track_durration(
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) -> Result<u64, String> {
    media_player
        .lock_unw()
        .use_guard(|guard| Ok(guard.get_len().as_secs()))
}

#[tauri::command]
pub async fn get_track_position(
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) -> Result<u64, String> {
    media_player
        .lock_unw()
        .use_guard(|guard| Ok(guard.get_pos().as_secs()))
}

#[tauri::command]
pub async fn get_current_volume(
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) -> Result<f32, String> {
    media_player
        .lock_unw()
        .use_guard(|guard| Ok(guard.get_volume()))
}

#[tauri::command]
pub async fn set_current_volume(
    media_player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
    volume: f32,
) -> Result<(), String> {
    media_player
        .lock_unw()
        .use_guard(|guard| Ok(guard.set_volume(volume)))
}
