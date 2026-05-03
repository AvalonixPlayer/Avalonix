use std::fmt::Display;

use crate::{
    media::{album::Album, track::Track},
    metadata::{album_filter_metadata::AlbumFilterMetadata, filter_metadata::FilterMetadata},
};
use anyhow::Ok;
use rkyv::{Archive, Deserialize, Serialize};

#[derive(Debug, Archive, Serialize, Deserialize, Clone)]
pub struct AlbumMetadata {
    pub id: Vec<u8>,
    pub album_title: String,
    pub album_performer: String,
    pub album_cover: String,
}

impl FilterMetadata for AlbumMetadata {
    type Output = AlbumFilterMetadata;
    fn get_filter_metadata(&self) -> anyhow::Result<Self::Output> {
        let result = AlbumFilterMetadata {
            id: self.id.clone(),
            title: self.album_title.clone(),
            artist: self.album_performer.clone(),
        };
        Ok(result)
    }
}

impl AlbumMetadata {
    pub fn from(id: &Vec<u8>, track: &Track, albums_hash: &Vec<Album>) -> Self {
        if let Some(album) = albums_hash
            .iter()
            .find(|x| x.album_metadata.album_title == track.metadata.album)
        {
            return album.album_metadata.clone();
        }
        AlbumMetadata {
            id: id.clone(),
            album_title: track.metadata.album.clone(),
            album_performer: track.metadata.artist.clone(),
            album_cover: track.get_cover_as_uri().unwrap_or_default(),
        }
    }
}

impl Display for AlbumMetadata {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        let cover_start: String = self.album_cover.chars().take(50).collect();
        write!(
            f,
            "\n\t\tid: {:?}\n\t\ttitle: {}\n\t\tperformer: {}\n\t\tcover_start: {}",
            self.id, self.album_title, self.album_performer, cover_start
        )
    }
}
