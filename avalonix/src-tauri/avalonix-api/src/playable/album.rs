use anyhow::bail;
use std::sync::{Arc, Mutex};
use uuid::Uuid;

use crate::{
    db::MusicDB,
    logger,
    media::metadata::{self, Metadata},
    playable::{library_part::LibraryPart, track::Track},
};

#[derive(ts_rs::TS, serde::Serialize, serde::Deserialize, Clone)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\Album.ts")]

pub struct Album {
    pub id: String,
    pub tracks_ids: Vec<Vec<u8>>,
    pub metadata: AlbumMetadata,
}

#[derive(ts_rs::TS, serde::Serialize, serde::Deserialize, Clone)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\Album.ts")]
pub struct AlbumMetadata {
    pub cover: Option<String>,
    pub name: String,
    pub artist: String,
}

impl Album {
    pub fn new(db: &MusicDB, tracks_ids: &Vec<Vec<u8>>, albums_hash: &Vec<Album>) -> Album {
        let first_trck = db.get_track_by_id(&tracks_ids[0]).unwrap();

        if let Some(album) = albums_hash
            .iter()
            .find(|x| x.metadata.name == *first_trck.metadata.album.as_ref().unwrap())
        {
            let mut album = album.clone();
            for (i, id) in album.tracks_ids.clone().iter().enumerate() {
                if tracks_ids.contains(id) {
                    album.tracks_ids.remove(i);
                }
            }
            return album;
        }

        Album {
            id: Uuid::new_v4().to_string(),
            tracks_ids: tracks_ids.to_vec(),
            metadata: AlbumMetadata {
                cover: None,
                name: first_trck.metadata.album.as_ref().unwrap().to_string(),
                artist: first_trck.metadata.artist.as_ref().unwrap().to_string(),
            },
        }
    }
}
