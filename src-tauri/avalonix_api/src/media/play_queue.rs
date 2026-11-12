use std::{
    sync::{Arc, Mutex, RwLock, mpsc::Sender},
    thread::{self, sleep},
    time::Duration,
};

use anyhow::{Context, Ok, Result, anyhow, bail};
use better_sms::mutex::{MutexGuardWork, MutexWork};
use rand::{rng, seq::SliceRandom};

use crate::{
    audio::media_player::MediaPlayer,
    disk::db::DB,
    events::Event,
    logger::{error, fatal, info, warn},
    media::playable_type::MediaType::Track,
};

pub struct PlayQueue {
    pub tracks_uuids_in_queue_displaying: Vec<String>,
    pub tracks_uuids_in_queue_real: Vec<String>,
    pub current_uuid_index: usize,
    pub shuffle: bool,
    pub media_player: Arc<Mutex<MediaPlayer>>,
    pub events_sender: Arc<Mutex<Sender<Event>>>,
    pub current_uuid: Option<String>,
    force_back: bool,
}

impl PlayQueue {
    pub fn new(
        media_player: &Arc<Mutex<MediaPlayer>>,
        events_sender: &Arc<Mutex<Sender<Event>>>,
    ) -> Self {
        Self {
            tracks_uuids_in_queue_displaying: vec![],
            tracks_uuids_in_queue_real: vec![],
            current_uuid_index: 0,
            shuffle: false,
            media_player: media_player.clone(),
            events_sender: events_sender.clone(),
            current_uuid: None,
            force_back: false,
        }
    }

    pub fn clear(&mut self) {
        self.current_uuid_index = 0;
        self.tracks_uuids_in_queue_displaying.clear();
        self.tracks_uuids_in_queue_real.clear();
        self.media_player.lock_unw().stop();
    }

    fn play_loop(
        queue: Arc<Mutex<Self>>,
        player: Arc<Mutex<MediaPlayer>>,
        db: Arc<RwLock<DB>>,
    ) -> Result<()> {
        let empty = player.lock_unw().empty();

        let mut queue = queue.lock_unw();

        if queue.tracks_uuids_in_queue_real.len() == 0 {
            queue.current_uuid_index = 0;
            return Ok(());
        }

        if !empty {
            return Ok(());
        }

        if !queue.force_back {
            if queue.tracks_uuids_in_queue_real.len() == 1 {
                queue.current_uuid_index += 0;
            } else {
                if queue.tracks_uuids_in_queue_real.len() > queue.current_uuid_index + 1 {
                    queue.current_uuid_index += 1;
                } else {
                    queue.current_uuid_index = 0;
                }
            }
        }

        if queue.current_uuid_index > queue.tracks_uuids_in_queue_real.len() {
            queue.current_uuid_index = queue.tracks_uuids_in_queue_real.len();
        }

        let uuid = queue
            .tracks_uuids_in_queue_real
            .get(queue.current_uuid_index as usize)
            .ok_or_else(|| anyhow!("Current index incorrect"))?
            .clone();
        queue.current_uuid = Some(uuid.clone());

        let track = db
            .read()
            .unwrap()
            .get_media_by_id(&uuid, Track)?
            .unwrap_as_track();

        player.lock_unw().play_track(&track)?;
        queue.force_back = false;
        Ok(())
    }

    pub fn play(queue: &Arc<Mutex<Self>>, db: &Arc<RwLock<DB>>) -> Result<()> {
        let queue = queue.clone();
        let player = queue.lock_unw().media_player.clone();
        let db = db.clone();
        thread::spawn(move || -> anyhow::Result<()> {
            loop {
                Self::play_loop(queue.clone(), player.clone(), db.clone())
                    .unwrap_or_else(|err| error(err));

                sleep(Duration::from_millis(100));
            }
            #[allow(unreachable_code)]
            Ok(())
        });
        Ok(())
    }

    pub fn add_tracks(&mut self, mut tracks_ids: Vec<String>) -> Result<()> {
        if self.tracks_uuids_in_queue_real.len() == 0 {
            self.force_back = true;
        }
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
        if let Some(index) = self
            .tracks_uuids_in_queue_real
            .iter()
            .position(|x| *x == uuid)
        {
            self.tracks_uuids_in_queue_real.remove(index);
            self.tracks_uuids_in_queue_displaying = self.tracks_uuids_in_queue_real.clone();
            if self.shuffle {
                self.tracks_uuids_in_queue_displaying.shuffle(&mut rng());
            }
            self.events_sender
                .lock_unw()
                .send(Event::UpdateQueue)
                .unwrap();
            if self.tracks_uuids_in_queue_real.len() == 0 {
                self.media_player.lock_unw().stop();
            }
            return Ok(());
        }
        bail!("not uuid")
    }

    pub fn next(&mut self) -> Result<()> {
        self.media_player.lock_unw().stop();
        Ok(())
    }

    pub fn back(&mut self) -> Result<()> {
        self.force_back = true;
        self.media_player.lock_unw().stop();
        if self.current_uuid_index > 0 {
            self.current_uuid_index -= 1;
        } else {
            self.current_uuid_index = 0;
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

    pub fn get_current_uuid(&mut self) -> Option<String> {
        self.current_uuid.clone()
    }

    pub fn start_by_uuid(&mut self, uuid: String) -> Result<()> {
        self.media_player.lock_unw().stop();
        if let Some(index) = self
            .tracks_uuids_in_queue_displaying
            .iter()
            .position(|x| *x == uuid)
        {
            self.force_back = true;
            self.current_uuid_index = index;
            Ok(())
        } else {
            bail!("no uuid in queue")
        }
    }
}
