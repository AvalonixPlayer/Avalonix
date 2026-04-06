pub mod commands;

use std::{
    sync::{
        mpsc::{self, Receiver, Sender},
        Arc, Mutex,
    },
    thread,
};

use avalonix_api::{
    audio::media_player::MediaPlayer,
    db::MusicDB,
    disk_manager, logger,
    playboxes::{
        albums_container::AlbumsContainer,
        artists_container::AristsContainer,
        play_queue::{PlayQueue, PlayQueueAction},
        playboxes::PlayboxesManager,
        tracks_container::TracksContainer,
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
            let play_queue_action_sender = api.2;
            let play_queue_action_compleated_reciver = api.3;
            let play_queue = api.4;
            let db = api.5;

            let player_clone = player.clone();

            tauri::Builder::default()
                .plugin(tauri_plugin_opener::init())
                .plugin(prevent_default())
                .setup(move |app| {
                    let (sender, reciver) = mpsc::channel();

                    let sender_clone = sender.clone();
                    player_clone.lock().unwrap().sender = Some(sender_clone);

                    let handle1 = app.app_handle().clone();
                    let handle2 = app.app_handle().clone();

                    thread::spawn(move || loop {
                        _ = handle1.emit("playing-track-updated", reciver.recv().unwrap());
                    });

                    thread::spawn(move || loop {
                        _ = handle2.emit(
                            "play-queue-action-compleated",
                            play_queue_action_compleated_reciver.recv().unwrap(),
                        );
                    });

                    Ok(())
                })
                .invoke_handler(tauri::generate_handler![
                    commands::get_all_tracks_id,
                    /*
                    commands::get_all_albums,
                    commands::get_all_artists,
                     */
                    commands::get_track_by_id,
                    commands::add_track_to_queue,
                    commands::clear_queue,
                    commands::remove_track_from_queue,
                    commands::get_queue,
                    commands::pause_or_continue,
                    commands::next_track,
                    commands::previous_track,
                    commands::play_track,
                    commands::on_pause
                ])
                .manage(player)
                .manage(playboxes_manager)
                .manage(play_queue_action_sender)
                .manage(play_queue)
                .manage(db)
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
        Arc<Mutex<Sender<PlayQueueAction>>>,
        Receiver<()>,
        Arc<Mutex<PlayQueue>>,
        MusicDB,
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

                let (playqueue_sender, playqueue_reciver) = mpsc::channel();

                let playqueue_sender_arc = Arc::new(Mutex::new(playqueue_sender));
                let playqueue_reciver_arc = Arc::new(Mutex::new(playqueue_reciver));

                let (playqueue_action_compleated_sender, playqueue_action_compleated_reciver) =
                    mpsc::channel();

                let playqueue_action_compleated_sender_arc =
                    Arc::new(Mutex::new(playqueue_action_compleated_sender));

                let play_queue = Arc::new(Mutex::new(PlayQueue::new(
                    &player,
                    &playqueue_reciver_arc,
                    &playqueue_action_compleated_sender_arc,
                )));

                PlayQueue::play(&play_queue);

                let tracks_container = TracksContainer::new(&db);
                //let albums_container = AlbumsContainer::new(&tracks_container, &db);
                //let artists_container = AristsContainer::new(&tracks_container);

                let playboxes_manager = PlayboxesManager::new(
                    tracks_container, /*, albums_container, artists_container*/
                );

                Ok((
                    player,
                    playboxes_manager,
                    playqueue_sender_arc,
                    playqueue_action_compleated_reciver,
                    play_queue,
                    db,
                ))
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
