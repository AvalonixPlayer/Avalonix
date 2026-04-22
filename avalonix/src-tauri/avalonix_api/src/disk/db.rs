use anyhow::{Ok, bail};
use rkyv::rancor::Error;

use crate::{disk::disk_manager, logger, media::track::Track};

pub struct DB {
    pub db_hash: DBHash,
    tracks: sled::Tree,
}

pub struct DBHash {
    pub tracks_hash: Vec<Track>,
}

impl DBHash {
    pub fn new() -> Self {
        Self {
            tracks_hash: vec![],
        }
    }
}

impl DB {
    pub fn open() -> anyhow::Result<Self> {
        let db_path = disk_manager::db_path();
        let db = sled::open(db_path)?;

        let tracks_tree = db.open_tree(b"tracks")?;

        let db = Self {
            tracks: tracks_tree,
            db_hash: DBHash::new(),
        };

        Ok(db)
    }
}

impl DB {
    pub fn save_track(&self, track: Track) -> anyhow::Result<()> {
        let key = &track.metadata.id;

        let value_bytes = rkyv::to_bytes::<Error>(&track)?.to_vec();

        self.tracks.insert(key, value_bytes)?;
        self.tracks.flush()?;

        logger::info(format!("track saved to db: {}", track));
        Ok(())
    }

    pub fn get_tracks_ids(&self) -> anyhow::Result<Vec<Vec<u8>>> {
        let mut result = vec![];
        for track in &self.tracks {
            let (id, _) = track?;
            result.push(id.to_vec());
        }
        Ok(result)
    }

    pub fn load_tracks_hash(&mut self) -> anyhow::Result<()> {
        let mut result = vec![];
        for track in &self.tracks {
            let (_, track) = track?;
            let track = rkyv::from_bytes::<Track, Error>(&track)?;
            result.push(track);
        }
        self.db_hash.tracks_hash = result;
        Ok(())
    }

    pub fn clear_tracks(&self) -> anyhow::Result<()> {
        self.tracks.clear()?;
        self.tracks.flush()?;
        Ok(())
    }
}

#[test]
fn test_db() -> anyhow::Result<()> {
    use crate::utils::get_argument_val;
    use std::path::PathBuf;

    let mut db = DB::open()?;

    let path_to_file = get_argument_val("FILE_PATH").unwrap();

    let path_to_file = PathBuf::from(path_to_file);

    if let Err(err) = db.load_tracks_hash() {
        bail!(err)
    }

    match Track::create_tracks_list_from_file(path_to_file, &db) {
        std::result::Result::Ok(tracks) => {
            for track in tracks {
                db.save_track(track)?;
            }
        }
        Err(err) => {
            logger::warn(err);
        }
    }

    if let Err(err) = db.load_tracks_hash() {
        bail!(err)
    }

    for track in db.db_hash.tracks_hash {
        logger::debug(format!("{}", track));
    }

    Ok(())
}

#[test]
fn test_clear_db() -> anyhow::Result<()> {
    let db = DB::open()?;
    db.clear_tracks()?;
    Ok(())
}
