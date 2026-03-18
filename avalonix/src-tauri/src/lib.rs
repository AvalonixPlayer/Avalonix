use std::{
    sync::{Arc, Mutex},
    thread,
    time::Duration,
};

use avalonix_api::{
    audio::media_player::{self, MediaPlayer},
    db::MusicDB,
    disk_manager, logger,
    playboxes::playboxes::{AlbumsContainer, AristsContainer, PlayboxesManager, TracksContainer},
};

#[cfg_attr(mobile, tauri::mobile_entry_point)]
//#[cfg(not(test))]

pub fn run() {
    init_api();
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}

fn init_api() -> Result<(Arc<Mutex<MediaPlayer>>), String> {
    let db_path = disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(&db_path);

    let media_player = MediaPlayer::new();

    match media_player {
        Ok(media_player) => {
            let player = Arc::new(Mutex::new(media_player));

            let player_clone1 = player.clone();
            let player_clone2 = player.clone();

            MediaPlayer::update(player_clone1);

            match db {
                Ok(db) => {
                    let tracks_container = TracksContainer::new(&db);
                    let albums_container = AlbumsContainer::new(&tracks_container);
                    let artists_container = AristsContainer::new(&tracks_container);

                    let playboxes_manager = PlayboxesManager::new(
                        tracks_container,
                        albums_container,
                        artists_container,
                    );
                    Ok((player_clone2))
                }
                Err(err) => {
                    logger::error(&err.to_string());
                    return Err(err.to_string().clone());
                }
            }
        }
        Err(err) => {
            logger::error(&err.to_string());
            return Err(err.to_string().clone());
        }
    }
}
