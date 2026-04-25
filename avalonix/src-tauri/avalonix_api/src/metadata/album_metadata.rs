use std::fmt::Display;

use crate::media::{album::Album, track::Track};
use rkyv::{Archive, Deserialize, Serialize};

#[derive(Debug, Archive, Serialize, Deserialize, Clone)]
pub struct AlbumMetadata {
    pub id: Vec<u8>,
    pub album_title: String,
    pub album_performer: String,
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
        }
    }
}

impl Display for AlbumMetadata {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "\n\t\tid: {:?}\n\t\ttitle: {}\n\t\tperformer: {}",
            self.id, self.album_title, self.album_performer
        )
    }
}
