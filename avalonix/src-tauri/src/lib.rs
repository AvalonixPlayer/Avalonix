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
            commands::get_tracks_filter_datas,
            commands::get_albums_ids,
            commands::start_track,
            commands::get_tracks_in_queue_ids,
            commands::get_cur_track_metadata,
            commands::get_track_cover,
            commands::update_library,
            commands::next_track,
            commands::previous_track,
            commands::pause_or_continue_track,
            commands::is_paused,
            commands::seek,
            commands::get_track_durration,
            commands::get_track_position,
            commands::get_albums_filter_datas,
            commands::get_albums_ids,
            commands::get_album_cover_by_id,
            commands::add_album_by_id,
            commands::get_performers_filter_datas,
            commands::get_performers_ids,
            commands::add_performer_by_id,
            commands::get_library_folders_from_settings,
            commands::add_folder_path_to_library,
            commands::clear_library,
            commands::remove_folder_path_from_library,
            commands::add_track_to_queue,
            commands::play_album_by_id
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
