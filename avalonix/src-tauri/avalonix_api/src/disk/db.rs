use anyhow::Ok;

use crate::{disk::disk_manager, media::track::Track};

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
    pub fn save_track(&mut self, track: Track) {}
}

#[test]
fn test_db() -> anyhow::Result<()> {
    let db = DB::open()?;

    Ok(())
}
