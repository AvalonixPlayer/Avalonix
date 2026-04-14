use std::sync::{Arc, Mutex};

use anyhow::Ok;

use crate::media::media_player::MediaPlayer;

pub struct Api {
    pub media_player: Arc<Mutex<MediaPlayer>>,
}

impl Api {
    pub fn new() -> anyhow::Result<Self> {
        let media_player = MediaPlayer::new()?;

        Ok(Self { media_player })
    }
}

pub fn init_api() -> anyhow::Result<Api> {
    Api::new()
}
