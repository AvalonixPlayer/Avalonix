use core::fmt;
use std::{
    collections::{HashMap, HashSet},
    format, vec,
};

use anyhow::Result;
use rkyv::{Archive, Deserialize, Serialize, hash::hash_value, rancor::Error};
use ts_rs::TS;
use uuid::Uuid;

use crate::{
    disk::db::DB,
    logger::{debug, error},
    media::{
        album,
        cover_get::CoverGet,
        media_array::MediaArray,
        media_trait::Media,
        playable_type::{MediaType, PlayableResult},
        track::Track,
    },
};

#[derive(Archive, Deserialize, Serialize, serde::Serialize, serde::Deserialize, TS, Clone)]
#[ts(export)]
pub struct Album {
    pub uuid: String,
    pub tracks_ids: Vec<String>,
    pub title: String,
    pub performer: String,
    pub cover_uri: String,
}

impl Album {
    pub fn create_albums(db: &DB, every_tracks_in_db: &[Track]) -> Result<()> {
        let mut every_albums_in_db = db.get_every_album()?;

        let mut exists_tracks: HashSet<&str> =
            every_tracks_in_db.iter().map(|t| t.uuid.as_str()).collect();

        let mut new_albums: HashMap<String, Album> = HashMap::new();

        for album in every_albums_in_db.iter_mut() {
            let orig_size = album.tracks_ids.len();

            album
                .tracks_ids
                .retain(|t| exists_tracks.contains(t.as_str()));

            // update if need
            if orig_size != album.tracks_ids.len() && album.tracks_ids.len() != 0 {
                let first_track = every_tracks_in_db
                    .iter()
                    .find(|t| t.uuid == album.tracks_ids[0])
                    .unwrap();
                album.update_album(first_track);
                db.add_to_db(album)?;
                debug(format!("{} updated", album.title.to_string()));
            }

            // remove if empty
            if album.tracks_ids.len() == 0 {
                debug(format!("{} removed", album.title.to_string()));
                db.remove_from_db(album)?;
            }

            // remove tracks, that now in albums
            for i in &album.tracks_ids {
                exists_tracks.remove(i.as_str());
            }
        }

        for track_uuid in exists_tracks.iter() {
            let track = every_tracks_in_db
                .iter()
                .find(|t| t.uuid == *track_uuid)
                .unwrap();

            if let Some(album) = new_albums.get_mut(&track.album) {
                album.tracks_ids.push(track_uuid.to_string());
            } else if let Some(album) = every_albums_in_db
                .iter_mut()
                .find(|a| a.title == track.album)
            {
                album.tracks_ids.push(track_uuid.to_string());
                album.update_album(track);

                db.add_to_db(album)?;
                debug(format!(
                    "{} updated with new track",
                    album.title.to_string()
                ));
            } else {
                let album = Self::create_new_album(track, vec![track_uuid.to_string()]);
                new_albums.insert(album.title.clone(), album);
            }
        }

        for album in new_albums {
            debug(format!("{} created", album.0));
            db.add_to_db(&album.1)?;
        }

        Ok(())
    }

    fn update_album(&mut self, first_track: &Track) {
        self.title = first_track.album.clone();
        self.performer = first_track.performer.clone();
        self.cover_uri = first_track.get_cover_as_uri();
    }

    pub fn create_new_album(first_track: &Track, tracks_ids: Vec<String>) -> Self {
        Self {
            uuid: Uuid::new_v4().to_string(),
            tracks_ids,
            title: first_track.album.clone(),
            performer: first_track.performer.clone(),
            cover_uri: first_track.get_cover_as_uri(),
        }
    }

    pub fn edit_metadata(&mut self, db: &DB, title: String, performer: String) -> Result<()> {
        self.title = title;
        self.performer = performer;

        for track_uuid in self.tracks_ids.iter() {
            let track = db
                .get_media_by_id(track_uuid.clone(), MediaType::Track)
                .unwrap()
                .unwrap_as_track();

            let title = track.title.clone();
            let genre = track.genre.clone();
            _ = db
                .edit_track(
                    track,
                    track_uuid.clone(),
                    title,
                    self.title.clone(),
                    self.performer.clone(),
                    genre,
                    None,
                )
                .map_err(|err| error(err.to_string()));
        }

        db.update_in_db(self)?;

        Ok(())
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

    fn name_starts_with<P: AsRef<str>>(&self, start: P) -> bool {
        self.title
            .to_lowercase()
            .starts_with(&start.as_ref().to_lowercase())
    }

    fn convert_to_db(&self) -> anyhow::Result<(Vec<u8>, Vec<u8>)> {
        let value = rkyv::to_bytes::<Error>(self)?.to_vec();
        let uuid = rkyv::to_bytes::<Error>(&self.uuid)?.to_vec();

        Ok((uuid, value))
    }

    fn get_tracks_uuids(&self) -> Vec<String> {
        self.tracks_ids.clone()
    }

    fn get_uuid(&self) -> String {
        self.uuid.clone()
    }
}

impl MediaArray for Album {
    fn add_track(&mut self, db: &DB, track_uuid: String) {
        self.tracks_ids.push(track_uuid);
        _ = db.update_in_db(self);
    }

    fn remove_track(&mut self, db: &DB, track_uuid: String) {
        let ind = self
            .tracks_ids
            .iter_mut()
            .position(|uuid| *uuid == track_uuid)
            .unwrap();
        self.tracks_ids.remove(ind);
        if self.tracks_ids.len() > 0 {
            _ = db.update_in_db(self);
        } else {
            _ = db.remove_from_db(self);
        }
    }

    fn create_new_for_track(db: &DB, track: &Track) {
        let album = Self::create_new_album(track, vec![track.uuid.clone()]);
        db.add_to_db(&album);
    }
}
