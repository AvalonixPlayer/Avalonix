use serde::Serialize;

#[derive(Serialize, ts_rs::TS)]
#[ts(export)]
pub struct AlbumFilterMetadata {
    pub id: Vec<u8>,
    pub title: String,
    pub artist: String,
}
