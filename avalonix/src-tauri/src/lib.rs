// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/

use std::thread;

use avalonix_api::{api::init_api, events::Event, logger};
use tauri::{Emitter, Manager};

pub mod commands;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    let api = init_api()
        .map_err(|err| logger::fatal(err))
        .expect("Error when create api");

    tauri::Builder::default()
        .setup(|app| {
            let app_handle = app.app_handle().clone();

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
            commands::get_tracks_in_queue_indexes,
            commands::get_cur_track_metadata,
            commands::get_track_cover,
            commands::update_tracks_library
        ])
        .manage(api.media_player)
        .manage(api.db)
        .manage(api.settings)
        .manage(api.play_queue)
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
