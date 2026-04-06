use std::sync::{Arc, Mutex};

use sled::IVec;

use crate::{db::MusicDB, logger, media::track::Track};

#[derive(ts_rs::TS)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\TracksContainer.ts")]
pub struct TracksContainer {
    pub all_tracks_id: Vec<Vec<u8>>,
}

impl TracksContainer {
    pub fn new(db: &MusicDB) -> TracksContainer {
        let ids = db.get_all_tracks_id().unwrap_or_else(|err| {
            logger::error(&err.to_string());
            Vec::new()
        });

        TracksContainer { all_tracks_id: ids }
    }

    pub fn get_track_by_id(&self, db: &MusicDB, id: Vec<u8>) -> Result<Track, ()> {
        let track = db.get_track_by_id(&id).unwrap_or_default();
        match track {
            Some(track) => Ok(track),
            None => Err(()),
        }
    }
}
