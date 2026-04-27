use std::sync::{Arc, Mutex};

use anyhow::Ok;

use crate::{
    disk::{db::DB, settings::Settings},
    media::{media_player::MediaPlayer, track::Track},
};

pub struct Api {
    pub media_player: Arc<Mutex<MediaPlayer>>,
    pub db: Mutex<DB>,
    pub settings: Mutex<Settings>,
}

impl Api {
    pub fn new() -> anyhow::Result<Self> {
        let media_player = MediaPlayer::new()?;
        let db = Mutex::new(DB::open()?);
        let settings = Mutex::new(Settings::open()?);

        {
            let mut db = db.lock().unwrap();
            db.load_tracks_hash()?;
            db.load_albums_hash()?;
        }

        Ok(Self {
            media_player,
            db,
            settings,
        })
    }
}

pub fn init_api() -> anyhow::Result<Api> {
    Api::new()
}
