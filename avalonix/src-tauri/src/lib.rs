// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/

use std::{sync::mpsc, thread};

use avalonix_api::{api::init_api, events::Event, logger};
pub mod commands;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    let api = init_api()
        .map_err(|err| logger::fatal(err))
        .expect("Error when create api");

    thread::spawn(move || loop {
        let ev = api.event_reciver.recv().unwrap();
        logger::debug("Event recived");

        match ev {
            Event::UpdatePlayingTrack => {}
        }
    });

    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![
            commands::get_tracks_filter_datas,
            commands::get_albums_ids,
            commands::start_track,
            commands::get_tracks_is_queue_indexes
        ])
        .manage(api.media_player)
        .manage(api.db)
        .manage(api.settings)
        .manage(api.play_queue)
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
