use std::{
    sync::{Arc, Mutex, mpsc::Sender},
    thread::{self, sleep},
    time::Duration,
};

use anyhow::{Context, Result};
use better_sms::mutex::MutexWork;
use rand::{rng, seq::SliceRandom};

use crate::{
    audio::media_player::MediaPlayer,
    disk::db::DB,
    events::Event,
    media::{
        media_trait::Media,
        playable_type::{self, MediaType},
    },
};

pub struct PlayQueue {
    pub tracks_uuids_in_queue_displaying: Vec<String>,
    pub tracks_uuids_in_queue_real: Vec<String>,
    pub current_uuid_index: i32,
    pub shuffle: bool,
    pub media_player: Arc<Mutex<MediaPlayer>>,
    pub events_sender: Arc<Mutex<Sender<Event>>>,
}

impl PlayQueue {
    pub fn new(
        media_player: &Arc<Mutex<MediaPlayer>>,
        events_sender: &Arc<Mutex<Sender<Event>>>,
    ) -> Self {
        Self {
            tracks_uuids_in_queue_displaying: vec![],
            tracks_uuids_in_queue_real: vec![],
            current_uuid_index: -1,
            shuffle: false,
            media_player: media_player.clone(),
            events_sender: events_sender.clone(),
        }
    }

    pub fn clear(&mut self) {
        self.current_uuid_index = -1;
        self.tracks_uuids_in_queue_displaying.clear();
        self.tracks_uuids_in_queue_real.clear();
        self.media_player.lock_unw().stop();
    }

    pub fn play(queue: &Arc<Mutex<Self>>, db: &Arc<Mutex<DB>>) -> Result<()> {
        let queue = queue.clone();
        let player = queue.lock_unw().media_player.clone();
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

    pub fn add_tracks(&mut self, mut tracks_ids: Vec<String>) -> Result<()> {
        for id in &tracks_ids {
            if let Some(position) = self.tracks_uuids_in_queue_real.iter().position(|x| x == id) {
                self.tracks_uuids_in_queue_real.remove(position);
            }
        }
        self.tracks_uuids_in_queue_real.append(&mut tracks_ids);
        self.tracks_uuids_in_queue_displaying = self.tracks_uuids_in_queue_real.clone();
        if self.shuffle {
            self.tracks_uuids_in_queue_displaying.shuffle(&mut rng());
        }
        self.events_sender
            .lock_unw()
            .send(Event::UpdateQueue)
            .unwrap();
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
        self.events_sender
            .lock_unw()
            .send(Event::UpdateQueue)
            .unwrap();
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
        self.events_sender
            .lock_unw()
            .send(Event::UpdateQueue)
            .unwrap();
        Ok(())
    }
}
