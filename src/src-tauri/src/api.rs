use std::sync::{
    mpsc::{self, Receiver},
    Arc, Mutex, RwLock,
};

use anyhow::Result;
use avalonix_api::{
    audio::media_player::MediaPlayer,
    disk::{db::DB, user::settings::UserSettings},
    events::Event,
    media::play_queue::PlayQueue,
};
use better_sms::{arc::ArcCreate, mutex::MutexCreate};

pub struct API {
    pub db: Arc<RwLock<DB>>,
    pub media_player: Arc<Mutex<MediaPlayer>>,
    pub queue: Arc<Mutex<PlayQueue>>,
    pub settings: Arc<Mutex<UserSettings>>,
    pub events_reciver: Receiver<Event>,
}

pub fn init_api() -> Result<API> {
    let (sender, reciver) = mpsc::channel::<Event>();

    let sender_arc = sender.create_mutex().create_arc();

    let db = RwLock::new(DB::open()?).create_arc();
    let media_player = MediaPlayer::new()?.create_mutex().create_arc();
    let queue = PlayQueue::new(&media_player, &sender_arc)
        .create_mutex()
        .create_arc();
    let settings = UserSettings::open()?.create_mutex().create_arc();

    PlayQueue::play(&queue, &db)?;

    Ok(API {
        db,
        media_player,
        queue,
        settings,
        events_reciver: reciver,
    })
}
