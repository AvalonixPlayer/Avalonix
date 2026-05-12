use std::{
    sync::{Arc, Mutex},
    thread,
    time::Duration,
};

use anyhow::bail;
use rand::seq::SliceRandom;
use rand::thread_rng;

use crate::{disk::db::DB, logger, media::media_player::MediaPlayer, mutex_work::CreateArcMutex};

/// Play queue structure
pub struct PlayQueue {
    pub player: Arc<Mutex<MediaPlayer>>,
    pub db: Arc<Mutex<DB>>,
    pub tracks_in_queue_ids: Vec<Vec<u8>>,
    real_tracks_ids: Vec<Vec<u8>>,
    pub cur_track_id: Vec<u8>,
    pub shuffle: bool,
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
            tracks_in_queue_ids: vec![],
            real_tracks_ids: vec![],
            cur_track_id: vec![],
            shuffle: false,
        }
        .create_arc_mutex();

        Self::update(&queue);

        Ok(queue)
    }

    /// Starts a background update
    fn update(play_queue: &Arc<Mutex<Self>>) {
        let play_queue = play_queue.clone();

        thread::spawn(move || {
            loop {
                thread::sleep(Duration::from_millis(100));

                let play_queue_guard = play_queue.lock().unwrap();
                let media_player = play_queue_guard.player.lock().unwrap();

                if media_player.is_empty() {
                    drop(media_player);
                    drop(play_queue_guard);

                    let mut play_queue_guard = play_queue.lock().unwrap();
                    _ = play_queue_guard.next().map_err(|err| logger::error(err));
                    _ = play_queue_guard
                        .start_track()
                        .map_err(|err| logger::error(err));
                }
            }
        });
    }

    /// Adds a track ID to the queue
    pub fn add_track(&mut self, track_id: Vec<u8>) -> anyhow::Result<()> {
        if !self.tracks_in_queue_ids.contains(&track_id) {
            self.tracks_in_queue_ids.push(track_id);
        }
        self.real_tracks_ids = self.tracks_in_queue_ids.clone();

        if self.shuffle {
            self.real_tracks_ids.shuffle(&mut thread_rng());
        }

        Ok(())
    }

    /// Adds album track IDs to the queue
    pub fn add_album(&mut self, album_id: Vec<u8>) -> anyhow::Result<()> {
        let db_guard = self.db.lock().unwrap();
        let mut tracks_ids = vec![];

        let albums_hash = db_guard.get_albums_hash()?;

        if let Some(album) = albums_hash.iter().find(|x| x.album_metadata.id == album_id) {
            for track_id in &album.tracks_ids {
                tracks_ids.push(track_id.clone());
            }
        }
        drop(db_guard);

        for id in tracks_ids {
            self.add_track(id)?;
        }
        Ok(())
    }

    /// Adds performer track IDs to the queue
    pub fn add_performer(&mut self, performer_id: Vec<u8>) -> anyhow::Result<()> {
        let db_guard = self.db.lock().unwrap();
        let mut tracks_ids = vec![];

        let performers_hash = db_guard.get_performers_hash()?;

        if let Some(performer) = performers_hash
            .iter()
            .find(|x| x.performer_metadata.id == performer_id)
        {
            for track_id in &performer.tracks_ids {
                tracks_ids.push(track_id.clone());
            }
        }
        drop(db_guard);

        for id in tracks_ids {
            self.add_track(id)?;
        }
        Ok(())
    }

    /// Сhanges the queue index
    pub fn next(&mut self) -> anyhow::Result<()> {
        let len = self.tracks_in_queue_ids.len();

        if let Some(pos_in_queue) = self
            .real_tracks_ids
            .iter()
            .position(|x| *x == self.cur_track_id)
        {
            if pos_in_queue + 1 < len {
                self.cur_track_id = self.real_tracks_ids[pos_in_queue + 1].clone();
            } else {
                if !self.tracks_in_queue_ids.is_empty() {
                    self.cur_track_id = self.real_tracks_ids[0].clone();
                } else {
                    let mut player_guard = self.player.lock().unwrap();
                    player_guard.stop_audio();
                    return Ok(());
                }
            }
        }
        self.start_track()?;

        Ok(())
    }

    /// Сhanges the queue index
    pub fn back(&mut self) -> anyhow::Result<()> {
        let len = self.tracks_in_queue_ids.len();

        if let Some(pos_in_queue) = self
            .real_tracks_ids
            .iter()
            .position(|x| *x == self.cur_track_id)
        {
            if pos_in_queue >= 1 {
                self.cur_track_id = self.real_tracks_ids[pos_in_queue - 1].clone();
            } else {
                if !self.tracks_in_queue_ids.is_empty() {
                    self.cur_track_id = self.real_tracks_ids[0].clone();
                } else {
                    let mut player_guard = self.player.lock().unwrap();
                    player_guard.stop_audio();
                    return Ok(());
                }
            }
        }
        self.start_track()?;

        Ok(())
    }

    /// Starts a track according to the queue index
    pub fn start_track(&self) -> anyhow::Result<()> {
        let mut player_guard = self.player.lock().unwrap();

        let hash = &self.db.lock().unwrap().get_tracks_hash()?;
        if let Some(track) = hash
            .iter()
            .find(|track| track.metadata.id == self.cur_track_id)
        {
            player_guard.start_audio(track)?;
            return Ok(());
        }
        player_guard.stop_audio();
        bail!("index don`t in hash");
    }

    /// Removes a track from queue
    pub fn remove_track(&mut self, id: Vec<u8>) -> anyhow::Result<()> {
        if let Some(position) = self
            .real_tracks_ids
            .iter()
            .position(|track_id| *track_id == id)
        {
            if id == self.cur_track_id {
                self.cur_track_id = vec![];
                let mut player_guard = self.player.lock().unwrap();
                player_guard.stop_audio();
            }
            if let Some(display_pos) = self.tracks_in_queue_ids.iter().position(|x| *x == id) {
                self.real_tracks_ids.remove(position);
                self.tracks_in_queue_ids.remove(display_pos);
                return Ok(());
            }
        }
        bail!("track not in queue");
    }

    /// Clears a queue
    pub fn clear_queue(&mut self) -> anyhow::Result<()> {
        let mut player_guard = self.player.lock().unwrap();
        player_guard.stop_audio();
        self.tracks_in_queue_ids.clear();
        Ok(())
    }

    pub fn shuffle_or_unshuffle(&mut self) -> bool {
        self.shuffle = !self.shuffle;
        if self.shuffle {
            self.real_tracks_ids.shuffle(&mut thread_rng());
        } else {
            self.real_tracks_ids = self.tracks_in_queue_ids.clone();
        }

        return self.shuffle;
    }
}
