use std::{
    sync::{Arc, Mutex},
    thread,
    time::Duration,
};

use anyhow::Ok;

use crate::{disk::db::DB, logger, media::media_player::MediaPlayer, mutex_work::CreateArcMutex};

/// Play queue structure
pub struct PlayQueue {
    pub player: Arc<Mutex<MediaPlayer>>,
    pub db: Arc<Mutex<DB>>,
    pub tracks_in_queue_indexes: Vec<usize>,
    pub cur_track_index: usize,
    playing_strted: bool,
}

impl PlayQueue {
    /// Creates a new instance of the playback queue
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

    /// Starts a background update
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

    /// Adds a track ID to the queue
    pub fn add_track(&mut self, index_in_library: usize) -> anyhow::Result<()> {
        if !self.tracks_in_queue_indexes.contains(&index_in_library) {
            self.tracks_in_queue_indexes.push(index_in_library);
        }

        Ok(())
    }

    /// Adds album track IDs to the queue
    pub fn add_album(&mut self, album_id: Vec<u8>) -> anyhow::Result<()> {
        let db_guard = self.db.lock().unwrap();
        let mut indexes = vec![];

        let albums_hash = db_guard.get_albums_hash()?;
        let tracks_hash = db_guard.get_tracks_hash()?;

        if let Some(album) = albums_hash.iter().find(|x| x.album_metadata.id == album_id) {
            for track_id in &album.tracks_ids {
                if let Some(track_index_in_lib) =
                    tracks_hash.iter().position(|x| x.metadata.id == *track_id)
                {
                    indexes.push(track_index_in_lib);
                }
            }
        }
        drop(db_guard);

        for index in indexes {
            self.add_track(index)?;
        }
        Ok(())
    }

    /// Adds performer track IDs to the queue
    pub fn add_performer(&mut self, performer_id: Vec<u8>) -> anyhow::Result<()> {
        let db_guard = self.db.lock().unwrap();
        let mut indexes = vec![];

        let performers_hash = db_guard.get_performers_hash()?;
        let tracks_hash = db_guard.get_tracks_hash()?;

        if let Some(performer) = performers_hash
            .iter()
            .find(|x| x.performer_metadata.id == performer_id)
        {
            for track_id in &performer.tracks_ids {
                if let Some(track_index_in_lib) =
                    tracks_hash.iter().position(|x| x.metadata.id == *track_id)
                {
                    indexes.push(track_index_in_lib);
                }
            }
        }
        drop(db_guard);

        for index in indexes {
            self.add_track(index)?;
        }
        Ok(())
    }

    /// Сhanges the queue index
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
        self.start_track()?;

        Ok(())
    }

    /// Сhanges the queue index
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

        self.start_track()?;

        Ok(())
    }

    /// Starts a track according to the queue index
    pub fn start_track(&self) -> anyhow::Result<()> {
        let mut player_guard = self.player.lock().unwrap();

        if let Some(_) = self
            .tracks_in_queue_indexes
            .iter()
            .find(|x| **x == self.cur_track_index)
        {
            let hash = &self.db.lock().unwrap().get_tracks_hash()?;
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
