use std::{
    collections::{HashMap, HashSet},
    sync::{Arc, Mutex},
    thread,
    time::Duration,
};

use anyhow::Ok;

use crate::{
    disk::db::{DB, DBHash},
    logger,
    media::media_player::MediaPlayer,
    mutex_work::CreateArcMutex,
};

pub struct PlayQueue {
    pub player: Arc<Mutex<MediaPlayer>>,
    pub library: DBHash,
    pub tracks_indexes: Vec<usize>,
    pub cur_track_index: usize,
    playing_strted: bool,
}

impl PlayQueue {
    pub fn new(
        player: &Arc<Mutex<MediaPlayer>>,
        library: &DBHash,
    ) -> anyhow::Result<Arc<Mutex<Self>>> {
        let queue = Self {
            player: player.clone(),
            library: library.clone(),
            tracks_indexes: vec![],
            cur_track_index: 0,
            playing_strted: false,
        }
        .create_arc_mutex();
        Ok(queue)
    }

    pub fn update(play_queue: &Arc<Mutex<Self>>) {
        let play_queue = play_queue.clone();

        thread::spawn(move || {
            loop {
                thread::sleep(Duration::from_millis(100));

                let play_queue_guard = play_queue.lock().unwrap();
                let media_player = play_queue_guard.player.lock().unwrap();

                if media_player.is_empty() {
                    drop(media_player);
                    drop(play_queue_guard);
                    {
                        let mut play_queue_guard = play_queue.lock().unwrap();
                        if play_queue_guard.playing_strted {
                            _ = play_queue_guard.next().map_err(|err| logger::error(err));
                            _ = play_queue_guard
                                .start_track()
                                .map_err(|err| logger::error(err));
                        } else {
                            _ = play_queue_guard
                                .start_track()
                                .map_err(|err| logger::error(err));
                            play_queue_guard.playing_strted = true;
                        }
                    };
                }
            }
        });
    }

    pub fn add_track(&mut self, index_in_library: usize) -> anyhow::Result<()> {
        if !self.tracks_indexes.contains(&index_in_library) {
            self.tracks_indexes.push(index_in_library);
        }

        Ok(())
    }

    pub fn next(&mut self) -> anyhow::Result<()> {
        let len = self.tracks_indexes.len();
        if self.cur_track_index + 1 < len {
            self.cur_track_index += 1;
        } else {
            self.cur_track_index = 0;
        }
        Ok(())
    }

    pub fn start_track(&self) -> anyhow::Result<()> {
        let mut player_guard = self.player.lock().unwrap();

        if let Some(index) = self.tracks_indexes.get(self.cur_track_index) {
            let track = &self.library.tracks_hash[*index];
            player_guard.start_audio(track)?;
            let len = player_guard.get_len();
        }

        Ok(())
    }
}

#[test]
fn test_play_queue() -> anyhow::Result<()> {
    let mut db = DB::open()?;
    db.load_tracks_hash()?;

    let player = MediaPlayer::new()?;

    let queue = PlayQueue::new(&player, &db.db_hash)?;

    PlayQueue::update(&queue);
    let mut queue_guard = queue.lock().unwrap();
    queue_guard.add_track(0)?;

    drop(queue_guard);

    loop {}
}
