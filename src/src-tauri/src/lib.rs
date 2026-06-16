use anyhow::Result;

use crate::api::init_api;

pub mod api;
pub mod commands;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() -> Result<()> {
    let api = init_api()?;
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![
            commands::get_tracks_ids,
            commands::get_albums_ids,
            commands::get_performers_ids,
            commands::get_track_by_id,
            commands::get_album_by_id,
            commands::get_performer_by_id,
            commands::get_album_performer_name_by_id,
        ])
        .manage(api.db)
        .manage(api.media_player)
        .manage(api.queue)
        .manage(api.settings)
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
    Ok(())
}
