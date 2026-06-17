use core::fmt;
use std::collections::{HashMap, HashSet};

use anyhow::Result;
use rkyv::{Archive, Deserialize, Serialize, rancor::Error};
use ts_rs::TS;
use uuid::Uuid;

use crate::{
    disk::db::DB,
    media::{cover_get::CoverGet, media_trait::Media, playable_type::MediaType, track::Track},
};

#[derive(Archive, Deserialize, Serialize, serde::Serialize, TS)]
#[ts(export)]
pub struct Album {
    pub uuid: String,
    pub tracks_ids: Vec<String>,
    pub title: String,
    pub performer: String,
    pub cover_uri: String,
}

impl Album {
    pub fn create_albums(db: &DB, every_tracks_in_db: &[Track]) -> Result<Vec<Self>> {
        let mut every_albums_in_db = db.get_every_album()?;

        let mut albums: HashMap<String, Vec<String>> = HashMap::new();
        for track in every_tracks_in_db {
            if let Some(album) = albums.iter_mut().find(|album| *album.0 == track.album) {
                album.1.push(track.uuid.clone());
            } else {
                albums.insert(track.album.clone(), vec![track.uuid.clone()]);
            }
        }

        let mut albums_needs_to_update = HashSet::new();

        for new_album in albums {
            if let Some(old_album) = every_albums_in_db
                .iter_mut()
                .find(|old_album| old_album.title == new_album.0)
            {
                if old_album.tracks_ids != new_album.1 {
                    old_album.tracks_ids = new_album.1;
                    albums_needs_to_update.insert(new_album.0);
                }
            } else {
                if let Some(track) = every_tracks_in_db
                    .iter()
                    .find(|track| *track.uuid == new_album.1[0])
                {
                    every_albums_in_db.push(Self::create_new_album(track, new_album.1)?);
                    albums_needs_to_update.insert(new_album.0);
                }
            }
        }

        for album_name in albums_needs_to_update {
            if let Some(album) = every_albums_in_db
                .iter_mut()
                .find(|album| *album.title == album_name)
            {
                db.add_to_db(album)?;
            }
        }

        Ok(every_albums_in_db)
    }

    fn create_new_album(track: &Track, tracks_ids: Vec<String>) -> Result<Self> {
        Ok(Self {
            uuid: Uuid::new_v4().to_string(),
            tracks_ids,
            title: track.album.clone(),
            performer: track.performer.clone(),
            cover_uri: track.get_cover_as_uri(),
        })
    }
}

impl fmt::Display for Album {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(
            f,
            "\talbum: {}\n\ttitle: {}\n\ttracks count: {}",
            self.uuid,
            self.title,
            self.tracks_ids.len()
        )
    }
}

impl Media for Album {
    fn get_media_type(&self) -> MediaType {
        MediaType::Album
    }
    fn convert_to_db(&self) -> anyhow::Result<(String, Vec<u8>)> {
        let value = rkyv::to_bytes::<Error>(self)?.to_vec();
        Ok((self.uuid.clone(), value))
    }
}
