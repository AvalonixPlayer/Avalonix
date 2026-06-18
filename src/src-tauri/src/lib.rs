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
            commands::get_playables_ids,
            commands::get_album_performer_name_by_id,
            commands::get_playable_by_id,
            commands::clear_queue,
            commands::add_media_to_queue,
        ])
        .manage(api.db)
        .manage(api.media_player)
        .manage(api.queue)
        .manage(api.settings)
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
    Ok(())
}
