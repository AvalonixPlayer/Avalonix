use std::{
    sync::{Arc, Mutex},
    thread,
    time::Duration,
};

use anyhow::Ok;

use crate::{
    disk::db::DBHash, logger, media::media_player::MediaPlayer, mutex_work::CreateArcMutex,
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

    pub fn add_track(&mut self, index: usize) -> anyhow::Result<()> {
        self.tracks_indexes.push(index);
        Ok(())
    }

    pub fn next(&mut self) -> anyhow::Result<()> {
        if self.cur_track_index + 1 < self.tracks_indexes.len() {
            self.cur_track_index += 1;
        } else {
            self.cur_track_index = 0;
        }
        self.start_track()?;
        Ok(())
    }

    pub fn start_track(&self) -> anyhow::Result<()> {
        let mut player_guard = self.player.lock().unwrap();
        let track = &self.library.tracks_hash[self.tracks_indexes[self.cur_track_index]];
        player_guard.start_audio(track)?;
        Ok(())
    }
}
