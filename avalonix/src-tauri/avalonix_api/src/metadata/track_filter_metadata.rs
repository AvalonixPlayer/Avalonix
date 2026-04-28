use serde::Serialize;

#[derive(Serialize, ts_rs::TS)]
#[ts(export)]
pub struct TrackFilterMetadata {
    pub id: Vec<u8>,
    pub title: String,
    pub album: String,
    pub artist: String,
    pub genre: String,
    pub bitrate: u32,
}

impl TrackFilterMetadata {}
