// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/

use std::thread;

use avalonix_api::{api::init_api, events::Event, logger};
use tauri::{Emitter, Manager};

use crate::tray::init_tray;

pub mod commands;
pub mod tray;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    let api = init_api()
        .map_err(|err| logger::fatal(err))
        .expect("Error when create api");

    tauri::Builder::default()
        .plugin(tauri_plugin_dialog::init())
        .setup(|app| {
            let app_handle = app.app_handle().clone();

            if let Err(err) = init_tray(app) {
                logger::error(err);
            }

            thread::spawn(move || loop {
                let ev = api.event_reciver.recv().unwrap();
                logger::debug("Event recived");

                match ev {
                    Event::UpdatePlayingTrack => _ = app_handle.emit("track-updated", ()),
                }
            });
            Ok(())
        })
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![
            // db_commands
            commands::db_commands::get_tracks_filter_datas,
            commands::db_commands::update_library,
            commands::db_commands::get_albums_filter_datas,
            commands::db_commands::get_albums_ids,
            commands::db_commands::get_album_cover_by_id,
            commands::db_commands::get_performers_filter_datas,
            commands::db_commands::get_performers_ids,
            commands::db_commands::clear_library,
            // media_player
            commands::media_player::pause_or_continue_track,
            commands::media_player::is_paused,
            commands::media_player::seek,
            commands::media_player::get_track_durration,
            commands::media_player::get_track_position,
            commands::media_player::get_current_volume,
            commands::media_player::set_current_volume,
            // queue
            commands::queue::start_track,
            commands::queue::next_track,
            commands::queue::previous_track,
            commands::queue::get_tracks_in_queue_ids,
            commands::queue::get_cur_track_metadata,
            commands::queue::get_track_cover,
            commands::queue::add_album_by_id,
            commands::queue::add_performer_by_id,
            commands::queue::add_track_to_queue,
            commands::queue::play_album_by_id,
            commands::queue::remove_track_from_queue,
            commands::queue::shuffle_or_unshuffle,
            commands::queue::shuffle_state,
            // settings
            commands::settings::get_library_folders_from_settings,
            commands::settings::add_folder_path_to_library,
            commands::settings::remove_folder_path_from_library,
        ])
        .manage(api.media_player)
        .manage(api.db)
        .manage(api.settings)
        .manage(api.play_queue)
        .on_window_event(|window, event| match event {
            tauri::WindowEvent::CloseRequested { api, .. } => {
                window.hide().unwrap();
                api.prevent_close();
            }
            _ => {}
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
