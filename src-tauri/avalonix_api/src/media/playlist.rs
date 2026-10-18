use std::todo;

use anyhow::{Result, bail};
use rkyv::{Archive, Deserialize, Serialize, rancor::Error};
use ts_rs::TS;
use uuid::Uuid;

use crate::{
    disk::db::DB,
    media::{media_array::MediaArray, media_trait::Media, playable_type::MediaType, track::Track},
};

#[derive(
    Debug, Archive, Serialize, Deserialize, serde::Serialize, serde::Deserialize, TS, Clone,
)]
#[ts(export)]
pub struct Playlist {
    pub uuid: String,
    pub tracks_ids: Vec<String>,
    pub title: String,
}

impl Playlist {
    fn new(name: String) -> Self {
        Self {
            uuid: Uuid::new_v4().to_string(),
            tracks_ids: Vec::new(),
            title: name,
        }
    }

    pub fn create_new_playlist(db: &DB, name: String) -> Result<Self> {
        if let Some(_) = db
            .get_every_playlist()?
            .iter()
            .find(|playlist| playlist.title == name)
        {
            bail!("playlist with this name exists")
        }

        let media = Self::new(name);
        db.add_to_db(&media)?;
        Ok(media)
    }
}

impl Media for Playlist {
    fn get_media_type(&self) -> MediaType {
        MediaType::Playlist
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

impl MediaArray for Playlist {
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
        todo!()
    }
}
