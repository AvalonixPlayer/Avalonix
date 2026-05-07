use std::{fmt::Display, time::Duration};

use anyhow::Ok;
use rkyv::{Archive, Deserialize, Serialize};

use crate::metadata::{
    filter_metadata::FilterMetadata, track_filter_metadata::TrackFilterMetadata,
};

#[derive(Debug, Archive, Serialize, serde::Serialize, Deserialize, Clone, ts_rs::TS)]
#[ts(export)]
pub struct TrackMetadata {
    pub id: Vec<u8>,
    pub file_path: String,
    #[ts(skip)]
    pub start_pos: Duration,
    #[ts(skip)]
    pub end_pos: Duration,
    pub title: String,
    pub album: String,
    pub artist: String,
    pub genre: String,
    pub bitrate: u32,
}

impl TrackMetadata {
    pub fn new(
        id: &Vec<u8>,
        file_path: &str,
        start_pos: Duration,
        end_pos: Duration,
        title: &str,
        album: &str,
        artist: &str,
        genre: &str,
        bitrate: u32,
    ) -> Self {
        Self {
            id: id.clone(),
            file_path: file_path.to_string(),
            start_pos,
            end_pos,
            title: title.to_string(),
            album: album.to_string(),
            artist: artist.to_string(),
            genre: genre.to_string(),
            bitrate,
        }
    }
}

impl Display for TrackMetadata {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "\n\t{:?}\n\tfile_path: \"{}\" \n\ttitle: {}\n\talbum: {}\n\tartist: {}\n\tgenre: {}\n\tstart: {}\n\tend: {}",
            self.id,
            self.file_path,
            self.title,
            self.album,
            self.artist,
            self.genre,
            self.start_pos.as_secs(),
            self.end_pos.as_secs()
        )
    }
}

impl FilterMetadata for TrackMetadata {
    type Output = TrackFilterMetadata;
    fn get_filter_metadata(&self) -> anyhow::Result<Self::Output> {
        let result = TrackFilterMetadata {
            id: self.id.clone(),
            title: self.title.clone(),
            album: self.album.clone(),
            artist: self.artist.clone(),
            genre: self.genre.clone(),
            bitrate: self.bitrate,
        };
        Ok(result)
    }
}
