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
        .plugin(tauri_plugin_opener::init())
        .setup(|app| {
            let app_handle = app.app_handle().clone();

            thread::spawn(move || loop {
                match api.events_reciver.recv().unwrap() {
                    Event::UpdateQueue => {
                        debug("queue-updated");
                        _ = app_handle.emit("queue-updated", ());
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
            commands::clear_queue,
            commands::add_media_to_queue,
            commands::get_queue_tracks_ids,
        ])
        .manage(api.db)
        .manage(api.media_player)
        .manage(api.queue)
        .manage(api.settings)
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
    Ok(())
}
