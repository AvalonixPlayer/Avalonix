// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/

use avalonix_api::{api::init_api, logger};
pub mod commands;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    let api = init_api()
        .map_err(|err| logger::fatal(err))
        .expect("Error when create api");

    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![
            commands::get_tracks_ids,
            commands::get_albums_ids,
        ])
        .manage(api.media_player)
        .manage(api.db)
        .manage(api.settings)
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
