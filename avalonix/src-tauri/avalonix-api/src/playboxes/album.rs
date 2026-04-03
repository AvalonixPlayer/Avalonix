use std::sync::{Arc, Mutex};
use uuid::Uuid;

use crate::{
    db::MusicDB,
    logger,
    media::{metadata::Metadata, track::Track},
};

#[derive(ts_rs::TS, serde::Serialize, serde::Deserialize, Clone)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\Album.ts")]

pub struct Album {
    pub id: String,
    pub tracks: Vec<Arc<Mutex<Track>>>,
    pub metadata: Option<AlbumMetadata>,
}

#[derive(ts_rs::TS, serde::Serialize, serde::Deserialize, Clone)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\Album.ts")]
pub struct AlbumMetadata {
    pub cover: Option<String>,
    pub name: String,
    pub artist: String,
}

impl Album {
    pub fn from(tracks: Vec<Arc<Mutex<Track>>>) -> Album {
        Album {
            id: Uuid::new_v4().to_string(),
            tracks: tracks,
            metadata: None,
        }
    }

    pub fn load_metadata(&mut self, db: &MusicDB, albums_hash: &Vec<&Album>, name: &String) {
        if let Some(album) = albums_hash
            .iter()
            .find(|x| x.metadata.as_ref().unwrap().name == *name)
        {
            logger::debug(&format!("album {} loaded from hash", name));
            self.metadata = album.metadata.clone();
        } else {
            logger::debug(&format!("album {} loaded without hash", name));
            self.metadata = Some(AlbumMetadata::from(&self.tracks));
            db.save_album(self).unwrap();
        }
    }
}

impl AlbumMetadata {
    fn from(tracks_arc: &Vec<Arc<Mutex<Track>>>) -> AlbumMetadata {
        let tracks = tracks_arc.clone();
        let first_track = tracks[0].clone();
        let first_track_guard = first_track.lock().unwrap();

        let album_cover = Metadata::get_cover(&first_track_guard.file_path)
            .map_err(|err| logger::error(&err))
            .unwrap_or(None);
        let result = AlbumMetadata {
            cover: album_cover,
            name: first_track_guard.metadata.album.as_ref().unwrap().clone(),
            artist: first_track_guard.metadata.artist.as_ref().unwrap().clone(),
        };

        result
    }
}
