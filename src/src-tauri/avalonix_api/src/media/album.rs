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

#[derive(Archive, Deserialize, Serialize, serde::Serialize, TS, Clone)]
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
        let every_albums_in_db = db.get_every_album()?;
        let mut albums_map: HashMap<String, Self> = every_albums_in_db
            .into_iter()
            .map(|album| (album.title.clone(), album))
            .collect();

        let existing_tracks_ids: HashSet<&str> =
            every_tracks_in_db.iter().map(|t| t.uuid.as_str()).collect();

        let mut albums_needs_to_update = Vec::new();

        for (album_title, album) in albums_map.iter_mut() {
            let original_len = album.tracks_ids.len();

            album
                .tracks_ids
                .retain(|id| existing_tracks_ids.contains(id.as_str()));

            if album.tracks_ids.len() != original_len {
                albums_needs_to_update.push(album_title.clone());
            }
        }

        let mut tracks_by_album: HashMap<String, Vec<String>> = HashMap::new();
        for track in every_tracks_in_db {
            tracks_by_album
                .entry(track.album.clone())
                .or_default()
                .push(track.uuid.clone());
        }

        let tracks_map: HashMap<&str, &Track> = every_tracks_in_db
            .iter()
            .map(|t| (t.uuid.as_str(), t))
            .collect();

        for (album_title, track_ids) in tracks_by_album {
            if let Some(old_album) = albums_map.get_mut(&album_title) {
                if old_album.tracks_ids != track_ids {
                    old_album.tracks_ids = track_ids;
                    if !albums_needs_to_update.contains(&album_title) {
                        albums_needs_to_update.push(album_title.clone());
                    }
                }
            } else {
                if let Some(first_track_id) = track_ids.first() {
                    if let Some(track) = tracks_map.get(first_track_id.as_str()) {
                        let new_album = Self::create_new_album(track, track_ids)?;
                        albums_map.insert(album_title.clone(), new_album);
                        albums_needs_to_update.push(album_title);
                    }
                }
            }
        }

        for album_name in &albums_needs_to_update {
            if let Some(album) = albums_map.get(album_name) {
                if album.tracks_ids.is_empty() {
                    db.remove_from_db(album)?;
                } else {
                    db.add_to_db(album)?;
                }
            }
        }

        Ok(albums_map.into_values().collect())
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

    fn convert_to_db(&self) -> anyhow::Result<(Vec<u8>, Vec<u8>)> {
        let value = rkyv::to_bytes::<Error>(self)?.to_vec();
        let uuid = rkyv::to_bytes::<Error>(&self.uuid)?.to_vec();

        Ok((uuid, value))
    }

    fn get_tracks_uuids(&self) -> Vec<String> {
        self.tracks_ids.clone()
    }
}
