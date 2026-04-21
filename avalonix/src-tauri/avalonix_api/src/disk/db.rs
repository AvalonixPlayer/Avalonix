use std::{ops::Deref, path::PathBuf};

use anyhow::Ok;
use rkyv::rancor::Error;

use crate::{disk::disk_manager, logger, media::track::Track, utils::get_argument_val};

pub struct DB {
    tracks: sled::Tree,
}

impl DB {
    pub fn open() -> anyhow::Result<Self> {
        let db_path = disk_manager::db_path();
        let db = sled::open(db_path)?;

        let tracks_tree = db.open_tree(b"tracks")?;

        let db = Self {
            tracks: tracks_tree,
        };

        Ok(db)
    }
}

impl DB {
    pub fn save_track(&mut self, track: Track) -> anyhow::Result<()> {
        let key = &track.id;

        let value_bytes = rkyv::to_bytes::<Error>(&track)?.to_vec();

        self.tracks.insert(key, value_bytes)?;
        self.tracks.flush()?;

        logger::info(format!("track saved to db: {}", track));
        Ok(())
    }
}

#[test]
fn test_db() -> anyhow::Result<()> {
    let mut db = DB::open()?;

    let path_to_file = get_argument_val("FILE_PATH").unwrap();

    let path_to_file = PathBuf::from(path_to_file);

    match Track::create_tracks_list_from_file(path_to_file) {
        std::result::Result::Ok(tracks) => {
            for track in tracks {
                db.save_track(track)?;
            }
        }
        Err(err) => {
            logger::warn(err);
        }
    }

    Ok(())
}
