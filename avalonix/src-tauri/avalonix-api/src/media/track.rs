use crate::db::MusicDB;

use super::metadata::Metadata;
use rkyv::{Archive, Deserialize, Serialize};
use uuid::Uuid;

#[derive(
    ts_rs::TS, serde::Serialize, serde::Deserialize, Archive, Deserialize, Serialize, Debug, Clone,
)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\Track.ts")]
pub struct Track {
    pub id: String,
    pub metadata: Metadata,
    pub file_path: String,
}

impl Track {
    pub fn new(file_path: &str, db: &MusicDB, tracks_hash: Vec<&Track>) -> Result<Self, String> {
        let metadata = Metadata::from(file_path, db, tracks_hash);
        match metadata {
            Ok(metadata) => Ok(Track {
                id: Uuid::new_v4().to_string(),
                metadata,
                file_path: file_path.to_string(),
            }),
            Err(err) => Err(err),
        }
    }

    pub fn from(file_path: &str, metadata: &Metadata) -> Self {
        Self {
            id: Uuid::new_v4().to_string(),
            metadata: metadata.clone(),
            file_path: file_path.to_string(),
        }
    }
}
