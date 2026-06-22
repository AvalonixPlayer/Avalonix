use std::sync::{Arc, Mutex};

use avalonix_api::audio::media_player::MediaPlayer;
use better_sms::mutex::MutexWork;

#[tauri::command]
pub async fn switch_media_player_state(
    player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) -> Result<(), String> {
    player.lock_unw().change_pause_state();
    Ok(())
}

#[tauri::command]
pub async fn get_media_player_state(
    player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) -> Result<bool, String> {
    Ok(player.lock_unw().is_paused())
}

#[tauri::command]
pub async fn get_media_player_end_pos(
    player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) -> Result<u64, String> {
    Ok(player.lock_unw().get_end_pos())
}

#[tauri::command]
pub async fn get_current_media_player_pos(
    player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
) -> Result<u64, String> {
    Ok(player.lock_unw().get_cur_pos())
}

#[tauri::command]
pub async fn try_seek(
    player: tauri::State<'_, Arc<Mutex<MediaPlayer>>>,
    pos: u64,
) -> Result<(), String> {
    player.lock_unw().seek(pos).map_err(|err| err.to_string())
}
