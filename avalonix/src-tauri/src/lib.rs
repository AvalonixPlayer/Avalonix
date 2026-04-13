// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/

use avalonix_api::{api::init_api, logger};

#[tauri::command]
fn greet(name: &str) -> String {
    format!("Hello, {}! You've been greeted from Rust!", name)
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    let api = init_api();
    if let Err(err) = api {
        logger::fatal(&err);
        panic!("{}", err);
    }

    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![greet])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
