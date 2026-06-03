use anyhow::Result;

use crate::{disk::disk_paths::avalonix_db, media::media_trait::Media};

/// Media database
pub struct DB {
    media_tree: sled::Tree,
}

impl DB {
    /// Opens avalonix media database
    pub fn open() -> Result<Self> {
        let db = sled::open(avalonix_db()?)?;
        let media_tree = db.open_tree("media")?;
        let result = Self { media_tree };
        Ok(result)
    }

    /// Adds media to db
    pub fn add_to_db(&self, media: &impl Media) -> Result<()> {
        let (key, value) = media.convert_to_db()?;
        self.media_tree.insert(key, value)?;
        Ok(())
    }
}
