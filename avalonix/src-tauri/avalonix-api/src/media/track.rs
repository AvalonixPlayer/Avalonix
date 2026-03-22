use super::metadata::Metadata;
use rkyv::{Archive, Deserialize, Serialize};
use uuid::Uuid;

#[derive(ts_rs::TS, serde::Serialize, Archive, Deserialize, Serialize, Debug, Clone)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\Track.ts")]
pub struct Track {
    pub id: String,
    pub metadata: Metadata,
    pub file_path: String,
}

impl Track {
    pub fn new(file_path: &str, metadata: Metadata) -> Self {
        Track {
            id: Uuid::new_v4().to_string(),
            metadata,
            file_path: file_path.to_string(),
        }
    }
}

impl PartialEq for Track {
    fn eq(&self, other: &Self) -> bool {
        self.id == other.id
    }
}
