use std::{
    path::{Path, PathBuf},
    sync::{Arc, Mutex},
};

use crate::{
    db::MusicDB,
    disk_manager, logger,
    playable::{playboxes::UpdateLib, track::Track, tracks_container},
    settings_manager::Settings,
};

#[derive(ts_rs::TS)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\TracksContainer.ts")]
pub struct TracksContainer {
    pub all_tracks_id: Vec<Vec<u8>>,
}

impl TracksContainer {
    pub fn new() -> TracksContainer {
        TracksContainer {
            all_tracks_id: Vec::new(),
        }
    }

    pub fn fill_ids(&mut self, db: &MusicDB) {
        let ids = db.get_all_tracks_id().unwrap_or_else(|err| {
            logger::error(&err.to_string());
            Vec::new()
        });

        self.all_tracks_id = ids;
    }

    pub fn get_track_by_id(&self, db: &MusicDB, id: Vec<u8>) -> Result<Track, ()> {
        let track = db.get_track_by_id(&id).unwrap_or_default();
        match track {
            Some(track) => Ok(track),
            None => Err(()),
        }
    }
}

impl UpdateLib for TracksContainer {
    fn update_lib(&self, db: &MusicDB, settings: &Settings) {
        let paths = disk_manager::get_all_tracks_paths(settings);
        let tracks_hash = db.get_all_tracks();
        match tracks_hash {
            Ok(tracks_hash) => {
                for file_path in paths {
                    let tracks_hash: Vec<&Track> = tracks_hash.iter().collect();

                    if !tracks_hash.iter().any(|x| x.file_path == file_path) {
                        let _ = Track::new(&file_path, db, tracks_hash);
                    }
                }

                for track in tracks_hash {
                    let p = Path::new(&track.file_path);
                    if !p.exists() || !p.is_file() {
                        _ = db.delete_track(&track.id);
                    }
                }
            }
            Err(err) => logger::error(&err.to_string()),
        }
    }
}
