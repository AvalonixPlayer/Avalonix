use std::sync::{Arc, Mutex};

use anyhow::Result;
use avalonix_api::{
    audio::media_player::MediaPlayer,
    disk::{
        db::DB,
        user::settings::{self, UserSettings},
    },
    media::play_queue::PlayQueue,
};
use better_sms::{
    arc::ArcCreate,
    mutex::{MutexCreate, MutexGuardWork, MutexWork},
};

pub struct API {
    pub db: Arc<Mutex<DB>>,
    pub media_player: Arc<Mutex<MediaPlayer>>,
    pub queue: Arc<Mutex<PlayQueue>>,
    pub settings: Arc<Mutex<UserSettings>>,
}

pub fn init_api() -> Result<API> {
    let db = DB::open()?.create_mutex().create_arc();
    let media_player = MediaPlayer::new()?.create_mutex().create_arc();
    let queue = PlayQueue::new().create_mutex().create_arc();
    let settings = UserSettings::open()?.create_mutex().create_arc();

    PlayQueue::play(&queue, &media_player, &db)?;

    Ok(API {
        db,
        media_player,
        queue,
        settings,
    })
}
