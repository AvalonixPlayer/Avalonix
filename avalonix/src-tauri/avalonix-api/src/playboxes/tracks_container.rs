use std::sync::{Arc, Mutex};

use crate::{db::MusicDB, disk_manager, logger, media::track::Track};

#[derive(ts_rs::TS)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\TracksContainer.ts")]
pub struct TracksContainer {
    pub all_tracks: Vec<Arc<Mutex<Track>>>,
}

impl TracksContainer {
    pub fn new(db: &MusicDB) -> TracksContainer {
        let all_tracks_paths = disk_manager::get_all_tracks_paths();
        let mut result_tracks: Vec<Arc<Mutex<Track>>> = Vec::new();

        for track_path in all_tracks_paths {
            let all_tracks = db.get_all_tracks().unwrap();
            let tracks_hash = all_tracks.iter().collect();

            let track = Track::new(&track_path, db, tracks_hash);
            match track {
                Ok(track) => result_tracks.push(Arc::new(Mutex::new(track))),
                Err(err) => logger::error(&err.to_string()),
            }
        }
        TracksContainer {
            all_tracks: result_tracks,
        }
    }
}
