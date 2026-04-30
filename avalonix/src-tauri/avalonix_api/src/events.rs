use std::sync::{
    Arc, Mutex,
    mpsc::{self, Receiver, Sender},
};

use crate::mutex_work::CreateArcMutex;

pub enum Event {
    UpdatePlayingTrack,
}
