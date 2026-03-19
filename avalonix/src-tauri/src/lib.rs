use std::{
    sync::{Arc, Mutex},
    thread,
    time::Duration,
};

use avalonix_api::{
    audio::media_player::{self, MediaPlayer},
    db::MusicDB,
    disk_manager, logger,
    playboxes::playboxes::{
        self, AlbumsContainer, AristsContainer, PlayboxesManager, TracksContainer,
    },
};
use tauri::{window, Emitter};

#[tauri::command]
fn get_all_tracks(window: tauri::Window, playboxes: tauri::State<'_, PlayboxesManager>) {
    let tracks = &playboxes.tracks_container.all_tracks;
    window.emit("get_all_tracks", tracks).unwrap();
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
//#[cfg(not(test))]

pub fn run() {
    let api = init_api();
    match api {
        Ok(api) => {
            let player = api.0;
            let playboxes_manager = api.1;

            let artists = &playboxes_manager.artists_container.artists;
            let albums = &playboxes_manager.albums_container.albums;

            let albums_names = albums.keys();

            for album in albums_names {
                println!("{}", album);
                let album_it = albums.get(album).unwrap();
                let first_track = &album_it[0];
                player.lock().unwrap().play(first_track.file_path.clone());
                break;
            }

            tauri::Builder::default()
                .plugin(tauri_plugin_opener::init())
                .invoke_handler(tauri::generate_handler![get_all_tracks])
                .manage(playboxes_manager)
                .run(tauri::generate_context!())
                .expect("error while running tauri application");
        }
        Err(err) => logger::error(&err),
    }
}

fn init_api() -> Result<(Arc<Mutex<MediaPlayer>>, PlayboxesManager), String> {
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

                    Ok((player_clone2, playboxes_manager))
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
