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
            commands::get_tracks_datas,
            commands::get_albums_datas,
            commands::get_performers_datas
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
    Ok(())
}
