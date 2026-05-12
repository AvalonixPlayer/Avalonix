use std::sync::{
    Arc, Mutex,
    mpsc::{self, Receiver},
};

use anyhow::Ok;

use crate::{
    disk::{db::DB, settings::Settings},
    events::Event,
    media::{media_player::MediaPlayer, play_queue::PlayQueue},
    mutex_work::CreateArcMutex,
};

pub struct Api {
    pub media_player: Arc<Mutex<MediaPlayer>>,
    pub play_queue: Arc<Mutex<PlayQueue>>,
    pub db: Arc<Mutex<DB>>,
    pub settings: Arc<Mutex<Settings>>,
    pub event_reciver: Receiver<Event>,
}

impl Api {
    pub fn new() -> anyhow::Result<Self> {
        let (event_sender, event_reciver) = mpsc::channel();

        let media_player = MediaPlayer::new(&event_sender)?;
        let db = DB::open()?;
        let settings = Settings::open()?.create_arc_mutex();
        let play_queue = PlayQueue::new(&media_player, &db)?;

        Ok(Self {
            media_player,
            play_queue,
            db,
            settings,
            event_reciver,
        })
    }
}

pub fn init_api() -> anyhow::Result<Api> {
    Api::new()
}
