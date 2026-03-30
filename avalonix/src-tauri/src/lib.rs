pub mod commands;

use std::{
    clone,
    sync::{mpsc, Arc, Mutex},
    thread,
};

use avalonix_api::{
    audio::media_player::MediaPlayer,
    db::MusicDB,
    disk_manager, logger,
    playboxes::{
        play_queue::PlayQueue,
        playboxes::{AlbumsContainer, AristsContainer, PlayboxesManager, TracksContainer},
    },
};
use tauri::{Emitter, Manager};

#[cfg_attr(mobile, tauri::mobile_entry_point)]

pub fn run() {
    let api = init_api();
    match api {
        Ok(api) => {
            let player = api.0;
            let playboxes_manager = api.1;
            let play_queue = api.2;

            let player_clone = player.clone();

            tauri::Builder::default()
                .plugin(tauri_plugin_opener::init())
                .plugin(prevent_default())
                .setup(move |app| {
                    let (sender, reciver) = mpsc::channel();
                    let sender_clone = sender.clone();
                    player_clone.lock().unwrap().sender = Some(sender_clone);

                    let handle = app.app_handle().clone();

                    thread::spawn(move || loop {
                        _ = handle.emit("playing-track-updated", reciver.recv().unwrap());
                    });

                    Ok(())
                })
                .invoke_handler(tauri::generate_handler![
                    commands::get_all_tracks,
                    commands::get_all_albums,
                    commands::get_all_artists,
                    commands::add_track_to_queue,
                    commands::clear_queue,
                    commands::remove_track_from_queue,
                    commands::get_queue,
                    commands::pause_or_continue,
                    commands::next_track,
                    commands::previous_track
                ])
                .manage(player)
                .manage(playboxes_manager)
                .manage(play_queue)
                .run(tauri::generate_context!())
                .expect("error while running tauri application");
        }
        Err(err) => logger::error(&err),
    }
}

fn init_api() -> Result<
    (
        Arc<Mutex<MediaPlayer>>,
        PlayboxesManager,
        Arc<Mutex<PlayQueue>>,
    ),
    String,
> {
    let db_path = disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(&db_path);

    let media_player = MediaPlayer::new();

    match media_player {
        Ok(media_player) => match db {
            Ok(db) => {
                let player = Arc::new(Mutex::new(media_player));

                MediaPlayer::update(&player);

                let play_queue = Arc::new(Mutex::new(PlayQueue::new(&player)));

                PlayQueue::play(&play_queue, &player);

                let tracks_container = TracksContainer::new(&db);
                let albums_container = AlbumsContainer::new(&tracks_container);
                let artists_container = AristsContainer::new(&tracks_container);

                let playboxes_manager =
                    PlayboxesManager::new(tracks_container, albums_container, artists_container);

                Ok((player, playboxes_manager, play_queue))
            }
            Err(err) => {
                logger::error(&err.to_string());
                return Err(err.to_string().clone());
            }
        },
        Err(err) => {
            logger::error(&err.to_string());
            return Err(err.to_string().clone());
        }
    }
}

#[cfg(debug_assertions)]
fn prevent_default() -> tauri::plugin::TauriPlugin<tauri::Wry> {
    use tauri_plugin_prevent_default::Flags;

    tauri_plugin_prevent_default::Builder::new()
        .with_flags(Flags::all().difference(Flags::DEV_TOOLS | Flags::RELOAD))
        .build()
}

#[cfg(not(debug_assertions))]
fn prevent_default() -> tauri::plugin::TauriPlugin<tauri::Wry> {
    tauri_plugin_prevent_default::init()
}
