use std::sync::{Arc, Mutex};

use anyhow::Ok;

use crate::{
    disk::{db::DB, settings::Settings},
    media::{media_player::MediaPlayer, play_queue::PlayQueue, track::Track},
};

pub struct Api {
    pub media_player: Arc<Mutex<MediaPlayer>>,
    pub play_queue: Arc<Mutex<PlayQueue>>,
    pub db: Mutex<DB>,
    pub settings: Mutex<Settings>,
}

impl Api {
    pub fn new() -> anyhow::Result<Self> {
        let media_player = MediaPlayer::new()?;
        let play_queue;
        let db = Mutex::new(DB::open()?);
        let settings = Mutex::new(Settings::open()?);

        {
            let mut db = db.lock().unwrap();
            db.load_tracks_hash()?;
            db.load_albums_hash()?;

            play_queue = PlayQueue::new(&media_player, &db.db_hash)?;
            PlayQueue::update(&play_queue);
        }

        Ok(Self {
            media_player,
            play_queue,
            db,
            settings,
        })
    }
}

pub fn init_api() -> anyhow::Result<Api> {
    Api::new()
}
