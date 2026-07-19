use std::thread;

use anyhow::Result;
use avalonix_api::{events::Event, logger::debug};
use tauri::{Emitter, Manager};

use crate::api::init_api;

pub mod api;
pub mod commands;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() -> Result<()> {
    let api = init_api()?;
    tauri::Builder::default()
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_opener::init())
        .setup(|app| {
            let app_handle = app.app_handle().clone();

            thread::spawn(move || loop {
                match api.events_reciver.recv().unwrap() {
                    Event::UpdateQueue => {
                        debug("queue-updated");
                        _ = app_handle.emit("queue-updated", ());
                    }
                    Event::UpdateLibrary => {
                        debug("library-updated");
                        _ = app_handle.emit("library-updated", ());
                    }
                    Event::CurTrackChanged => {
                        debug("track-updated");
                        _ = app_handle.emit("cur-track-changed", ());
                    }
                }
                debug("Event recived");
            });

            Ok(())
        })
        .invoke_handler(tauri::generate_handler![
            commands::get_playables_ids,
            commands::get_album_performer_name_by_id,
            commands::get_playable_by_id,
            commands::update_library,
            commands::clear_queue,
            commands::add_media_to_queue,
            commands::get_queue_tracks_ids,
            commands::next_track,
            commands::previous_track,
            commands::switch_media_player_state,
            commands::get_media_player_state,
            commands::get_media_player_end_pos,
            commands::get_current_media_player_pos,
            commands::try_seek,
            commands::get_current_track_uuid,
            commands::get_track_cover,
            commands::get_library_paths,
            commands::remove_path_from_lib,
            commands::add_path_to_lib,
            commands::get_theme,
            commands::save_settings,
            commands::set_theme,
            commands::get_bg_gif_uri
        ])
        .manage(api.db)
        .manage(api.media_player)
        .manage(api.queue)
        .manage(api.settings)
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
    Ok(())
}
