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

#[derive(Archive, Deserialize, Serialize, serde::Serialize, TS)]
#[ts(export)]
pub struct Performer {
    pub uuid: String,
    pub tracks_ids: Vec<String>,
    pub title: String,
}

impl Performer {
    pub fn create_performers(db: &DB, every_tracks_in_db: &[Track]) -> Result<Vec<Self>> {
        let mut every_performers_in_db = db.get_every_performer()?;

        let mut performers: HashMap<String, Vec<String>> = HashMap::new();
        for track in every_tracks_in_db {
            if let Some(performer) = performers
                .iter_mut()
                .find(|performer| *performer.0 == track.performer)
            {
                performer.1.push(track.uuid.clone());
            } else {
                performers.insert(track.performer.clone(), vec![track.uuid.clone()]);
            }
        }

        let mut performers_needs_to_update = HashSet::new();

        for new_performer in performers {
            if let Some(old_performer) = every_performers_in_db
                .iter_mut()
                .find(|old_performer| old_performer.title == new_performer.0)
            {
                if old_performer.tracks_ids != new_performer.1 {
                    old_performer.tracks_ids = new_performer.1;
                    performers_needs_to_update.insert(new_performer.0);
                }
            } else {
                if let Some(track) = every_tracks_in_db
                    .iter()
                    .find(|track| *track.uuid == new_performer.1[0])
                {
                    every_performers_in_db
                        .push(Self::create_new_performer(track, new_performer.1)?);
                    performers_needs_to_update.insert(new_performer.0);
                }
            }
        }

        for performer_name in performers_needs_to_update {
            if let Some(performer) = every_performers_in_db
                .iter_mut()
                .find(|performer| *performer.title == performer_name)
            {
                db.add_to_db(performer)?;
            }
        }

        Ok(every_performers_in_db)
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
    fn convert_to_db(&self) -> anyhow::Result<(String, Vec<u8>)> {
        let value = rkyv::to_bytes::<Error>(self)?.to_vec();
        Ok((self.uuid.clone(), value))
    }
}
