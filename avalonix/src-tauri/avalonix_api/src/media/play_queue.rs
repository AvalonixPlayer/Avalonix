use std::{
    collections::{HashMap, HashSet},
    sync::{Arc, Mutex, mpsc},
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
    pub db: Arc<Mutex<DB>>,
    pub tracks_in_queue_indexes: Vec<usize>,
    pub cur_track_index: usize,
    playing_strted: bool,
}

impl PlayQueue {
    pub fn new(
        player: &Arc<Mutex<MediaPlayer>>,
        db: &Arc<Mutex<DB>>,
    ) -> anyhow::Result<Arc<Mutex<Self>>> {
        let queue = Self {
            player: player.clone(),
            db: db.clone(),
            tracks_in_queue_indexes: vec![],
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
        if !self.tracks_in_queue_indexes.contains(&index_in_library) {
            self.tracks_in_queue_indexes.push(index_in_library);
        }

        Ok(())
    }

    pub fn next(&mut self) -> anyhow::Result<()> {
        let len = self.tracks_in_queue_indexes.len();
        if let Some(index_in_queue) = self
            .tracks_in_queue_indexes
            .iter()
            .position(|x| *x == self.cur_track_index)
        {
            if index_in_queue + 1 < len {
                self.cur_track_index = self.tracks_in_queue_indexes[index_in_queue + 1];
            } else {
                if !self.tracks_in_queue_indexes.is_empty() {
                    self.cur_track_index = self.tracks_in_queue_indexes[0];
                } else {
                    let mut player_guard = self.player.lock().unwrap();
                    player_guard.stop_audio();
                }
            }
        }

        Ok(())
    }

    pub fn back(&mut self) -> anyhow::Result<()> {
        if let Some(index_in_queue) = self
            .tracks_in_queue_indexes
            .iter()
            .position(|x| *x == self.cur_track_index)
        {
            if index_in_queue as i32 - 1 >= 0 {
                self.cur_track_index = self.tracks_in_queue_indexes[index_in_queue - 1];
            } else {
                if !self.tracks_in_queue_indexes.is_empty() {
                    self.cur_track_index = self.tracks_in_queue_indexes[0];
                } else {
                    let mut player_guard = self.player.lock().unwrap();
                    player_guard.stop_audio();
                }
            }
        }

        Ok(())
    }

    pub fn start_track(&self) -> anyhow::Result<()> {
        let mut player_guard = self.player.lock().unwrap();

        if let Some(_) = self
            .tracks_in_queue_indexes
            .iter()
            .find(|x| **x == self.cur_track_index)
        {
            let hash = &self.db.lock().unwrap().db_hash.tracks_hash;
            let track = hash.get(self.cur_track_index);
            match track {
                Some(track) => {
                    player_guard.start_audio(track)?;
                }
                None => {
                    logger::error("index don`t in hash");
                }
            }
        } else {
            player_guard.stop_audio();
        }

        Ok(())
    }
}

#[test]
fn test_play_queue() -> anyhow::Result<()> {
    let db = DB::open()?;
    let mut db_guard = db.lock().unwrap();
    db_guard.load_tracks_hash()?;
    drop(db_guard);

    let (event_sender, _) = mpsc::channel();

    let player = MediaPlayer::new(&event_sender)?;

    let queue = PlayQueue::new(&player, &db)?;

    PlayQueue::update(&queue);
    let mut queue_guard = queue.lock().unwrap();
    queue_guard.add_track(0)?;

    drop(queue_guard);

    loop {}
}
