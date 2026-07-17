use core::fmt;
use std::collections::{HashMap, HashSet};

use anyhow::Result;
use rkyv::{Archive, Deserialize, Serialize, rancor::Error};
use ts_rs::TS;
use uuid::Uuid;

use crate::{
    disk::db::DB,
    media::{media_trait::Media, playable_type::MediaType, track::Track},
};

#[derive(Archive, Deserialize, Serialize, serde::Serialize, TS, Clone)]
#[ts(export)]
pub struct Performer {
    pub uuid: String,
    pub tracks_ids: Vec<String>,
    pub title: String,
}

impl Performer {
    pub fn create_performers(db: &DB, every_tracks_in_db: &[Track]) -> Result<Vec<Self>> {
        let every_performers_in_db = db.get_every_performer()?;
        let mut performers_map: HashMap<String, Self> = every_performers_in_db
            .into_iter()
            .map(|performer| (performer.title.clone(), performer))
            .collect();

        let existing_tracks_ids: HashSet<&str> =
            every_tracks_in_db.iter().map(|t| t.uuid.as_str()).collect();

        let mut performers_needs_to_update = Vec::new();

        for (performer_title, performer) in performers_map.iter_mut() {
            let original_len = performer.tracks_ids.len();

            performer
                .tracks_ids
                .retain(|id| existing_tracks_ids.contains(id.as_str()));

            if performer.tracks_ids.len() != original_len {
                performers_needs_to_update.push(performer_title.clone());
            }
        }

        let mut tracks_by_performer: HashMap<String, Vec<String>> = HashMap::new();
        for track in every_tracks_in_db {
            tracks_by_performer
                .entry(track.performer.clone())
                .or_default()
                .push(track.uuid.clone());
        }

        let tracks_map: HashMap<&str, &Track> = every_tracks_in_db
            .iter()
            .map(|t| (t.uuid.as_str(), t))
            .collect();

        for (performer_title, track_ids) in tracks_by_performer {
            if let Some(old_performer) = performers_map.get_mut(&performer_title) {
                if old_performer.tracks_ids != track_ids {
                    old_performer.tracks_ids = track_ids;
                    if !performers_needs_to_update.contains(&performer_title) {
                        performers_needs_to_update.push(performer_title.clone());
                    }
                }
            } else {
                if let Some(first_track_id) = track_ids.first() {
                    if let Some(track) = tracks_map.get(first_track_id.as_str()) {
                        let new_performer = Self::create_new_performer(track, track_ids)?;
                        performers_map.insert(performer_title.clone(), new_performer);
                        performers_needs_to_update.push(performer_title);
                    }
                }
            }
        }

        for performer_name in &performers_needs_to_update {
            if let Some(performer) = performers_map.get(performer_name) {
                if performer.tracks_ids.is_empty() {
                    db.remove_from_db(performer)?;
                } else {
                    db.add_to_db(performer)?;
                }
            }
        }

        Ok(performers_map
            .into_values()
            .filter(|performer| !performer.tracks_ids.is_empty())
            .collect())
    }

    fn create_new_performer(track: &Track, tracks_ids: Vec<String>) -> Result<Self> {
        Ok(Self {
            uuid: Uuid::new_v4().to_string(),
            tracks_ids,
            title: track.performer.clone(),
        })
    }
}

impl fmt::Display for Performer {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(
            f,
            "\tperformer: {}\n\ttitle: {}\n\ttracks count: {}",
            self.uuid,
            self.title,
            self.tracks_ids.len()
        )
    }
}

impl Media for Performer {
    fn get_media_type(&self) -> MediaType {
        MediaType::Performer
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
