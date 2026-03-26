use std::{
    fmt,
    sync::{Arc, Mutex},
    thread,
    time::Duration,
};

use crate::{
    audio::media_player::{self, MediaPlayer},
    db::MusicDB,
    disk_manager, logger,
    media::track::Track,
    playboxes::playboxes::TracksContainer,
};

#[derive(Debug, ts_rs::TS)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\PlayQueue.ts")]
pub struct PlayQueue {
    pub tracks: Vec<Arc<Track>>,
    current_track_index: usize,
}

impl PlayQueue {
    pub fn new() -> Self {
        PlayQueue {
            tracks: Vec::new(),
            current_track_index: 0,
        }
    }

    pub fn clear(&mut self) {
        logger::debug("queue cleared");
        self.tracks.clear();
    }

    pub fn add_track(&mut self, track: Arc<Track>) {
        for i in self.tracks.clone() {
            if i == track {
                logger::warn(&format!(
                    "track with id: {} also in play queue",
                    track.id.clone()
                ));
                return;
            };
        }

        logger::debug(&format!(
            "track with id: {} added to play queue",
            track.id.clone()
        ));
        self.tracks.push(track);
    }

    pub fn add_tracks(&mut self, tracks: Vec<Arc<Track>>) {
        for track in tracks {
            self.add_track(track);
        }
    }

    pub fn remove_track(&mut self, track: Arc<Track>) {
        let mut index = 0;
        for i in self.tracks.clone() {
            if i == track {
                self.tracks.remove(index);
                logger::debug(&format!("track with id: {} removed", track.id));
                return;
            }
            index += 1;
        }
        logger::warn(&format!("track with id: {} not found in queue", track.id));
    }

    pub fn play(self_arc: &Arc<Mutex<Self>>, media_player_arc: &Arc<Mutex<MediaPlayer>>) {
        let self_clone = self_arc.clone();
        let media_player_clone = media_player_arc.clone();

        thread::spawn(move || {
            loop {
                {
                    let self_guard = self_clone.try_lock();
                    match self_guard {
                        Ok(mut self_guard) => {
                            let media_player_guard = media_player_clone.try_lock();
                            match media_player_guard {
                                Ok(mut media_player_guard) => {
                                    if self_guard.tracks.len() != 0 {
                                        if media_player_guard.empty() {
                                            if self_guard.current_track_index + 1
                                                <= self_guard.tracks.len() - 1
                                            {
                                                self_guard.current_track_index += 1;
                                                let track = &self_guard.tracks
                                                    [self_guard.current_track_index];
                                                media_player_guard.play(track.file_path.clone());
                                            } else {
                                                self_guard.current_track_index = 0;
                                                let track = &self_guard.tracks
                                                    [self_guard.current_track_index];
                                                media_player_guard.play(track.file_path.clone());
                                            }
                                        }
                                    }
                                }
                                Err(err) => logger::acceptable_error(&err.to_string()),
                            }
                        }
                        Err(err) => logger::acceptable_error(&err.to_string()),
                    }
                }
                thread::sleep(Duration::new(0, 1000));
            }
        });
    }

    pub fn pause_or_continue(&mut self, media_player_guard: Arc<Mutex<MediaPlayer>>) {
        let media_player;
        match media_player_guard.try_lock() {
            Ok(mp) => {
                media_player = mp;

                match media_player.is_paused() {
                    true => {
                        media_player.cont();
                    }
                    false => {
                        media_player.pause();
                    }
                }
            }
            Err(err) => logger::error(&err.to_string()),
        }
    }
}

#[test]
fn test_play_queue() {
    let hash_path = disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(&hash_path).unwrap();
    let cont = TracksContainer::new(&db);

    let mp = MediaPlayer::new().unwrap();

    let media_player = Arc::new(Mutex::new(mp));
    MediaPlayer::update(&media_player);

    let queue = Arc::new(Mutex::new(PlayQueue::new()));
    {
        let mut queue_guard = queue.lock().unwrap();

        queue_guard.add_tracks(cont.all_tracks.clone());
    }

    PlayQueue::play(&queue, &media_player);

    loop {}
}
