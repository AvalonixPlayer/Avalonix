use std::{
    sync::{Arc, Mutex},
    thread::{self, sleep},
    time::Duration,
};

use anyhow::{Context, Result};
use rand::{rng, seq::SliceRandom};

use crate::{audio::media_player::MediaPlayer, disk::db::DB};

pub struct PlayQueue {
    pub tracks_uuids_in_queue_displaying: Vec<String>,
    pub tracks_uuids_in_queue_real: Vec<String>,
    pub current_uuid_index: i32,
    pub shuffle: bool,
}

impl PlayQueue {
    pub fn new() -> Self {
        Self {
            tracks_uuids_in_queue_displaying: vec![],
            tracks_uuids_in_queue_real: vec![],
            current_uuid_index: -1,
            shuffle: false,
        }
    }

    pub fn play(
        queue: &Arc<Mutex<Self>>,
        player: &Arc<Mutex<MediaPlayer>>,
        db: &Arc<Mutex<DB>>,
    ) -> Result<()> {
        let queue = queue.clone();
        let player = player.clone();
        let db = db.clone();
        thread::spawn(move || -> anyhow::Result<()> {
            let mut rng = rng();
            loop {
                let is_empty = {
                    let player_lock = player.lock().unwrap();
                    player_lock.empty()
                };

                if is_empty {
                    let current_uuid = {
                        let mut queue = queue.lock().unwrap();
                        if queue.tracks_uuids_in_queue_real.is_empty() {
                            None
                        } else {
                            if queue.current_uuid_index == -1 {
                                if queue.shuffle {
                                    queue.tracks_uuids_in_queue_displaying.shuffle(&mut rng);
                                }
                                queue.current_uuid_index = 0;
                            } else {
                                if queue.current_uuid_index as usize + 1
                                    >= queue.tracks_uuids_in_queue_real.len()
                                {
                                    queue.current_uuid_index = 0;
                                    if queue.shuffle {
                                        queue.tracks_uuids_in_queue_displaying.shuffle(&mut rng);
                                    }
                                } else {
                                    queue.current_uuid_index += 1;
                                }
                            }
                            Some(
                                queue.tracks_uuids_in_queue_displaying
                                    [queue.current_uuid_index as usize]
                                    .clone(),
                            )
                        }
                    };

                    if let Some(uuid) = current_uuid {
                        let track_path = {
                            let db_lock = db.lock().unwrap();
                            db_lock
                                .get_every_track()?
                                .into_iter()
                                .find(|track| track.uuid == uuid)
                                .context("Track can't be found in DB")?
                                .path
                        };

                        let player_lock = player.lock().unwrap();
                        player_lock.play_audio_file(&track_path)?;
                    }
                }

                sleep(Duration::from_millis(100));
            }
            #[allow(unreachable_code)]
            Ok(())
        });
        Ok(())
    }

    pub fn add_track(&mut self, uuid: String) -> Result<()> {
        self.tracks_uuids_in_queue_real.push(uuid);
        self.tracks_uuids_in_queue_displaying = self.tracks_uuids_in_queue_real.clone();
        if self.shuffle {
            self.tracks_uuids_in_queue_displaying.shuffle(&mut rng());
        }
        Ok(())
    }

    pub fn remove_track(&mut self, uuid: String) -> Result<()> {
        let index = self
            .tracks_uuids_in_queue_real
            .iter()
            .position(|x| *x == uuid)
            .unwrap();
        self.tracks_uuids_in_queue_real.remove(index);
        self.tracks_uuids_in_queue_displaying = self.tracks_uuids_in_queue_real.clone();
        if self.shuffle {
            self.tracks_uuids_in_queue_displaying.shuffle(&mut rng());
        }
        Ok(())
    }

    pub fn next(&mut self, player: &Arc<Mutex<MediaPlayer>>) -> Result<()> {
        player.clone().lock().unwrap().stop();
        Ok(())
    }

    pub fn back(&mut self, player: &Arc<Mutex<MediaPlayer>>) -> Result<()> {
        player.clone().lock().unwrap().stop();
        if self.current_uuid_index - 1 >= 0 {
            self.current_uuid_index -= 1;
        } else {
            self.current_uuid_index = self.tracks_uuids_in_queue_real.len() as i32;
        }
        Ok(())
    }

    pub fn switch_shuffle(&mut self) -> Result<()> {
        self.shuffle = !self.shuffle;
        if self.shuffle {
            self.tracks_uuids_in_queue_displaying.shuffle(&mut rng());
        } else {
            self.tracks_uuids_in_queue_displaying = self.tracks_uuids_in_queue_real.clone();
        }
        Ok(())
    }
}
