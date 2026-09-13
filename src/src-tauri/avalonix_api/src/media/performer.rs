use core::fmt;
use std::collections::{HashMap, HashSet};

use anyhow::Result;
use rkyv::{Archive, Deserialize, Serialize, rancor::Error};
use ts_rs::TS;
use uuid::Uuid;

use crate::{
    disk::db::DB,
    logger::{debug, error},
    media::{
        media_array::MediaArray,
        media_trait::Media,
        playable_type::{MediaType, PlayableResult},
        track::Track,
    },
};

#[derive(Archive, Deserialize, Serialize, serde::Serialize, serde::Deserialize, TS, Clone)]
#[ts(export)]
pub struct Performer {
    pub uuid: String,
    pub tracks_ids: Vec<String>,
    pub title: String,
}

impl Performer {
    pub fn create_performers(db: &DB, every_tracks_in_db: &[Track]) -> Result<()> {
        let mut every_performers_in_db = db.get_every_performer()?;

        let mut exists_tracks: HashSet<&str> =
            every_tracks_in_db.iter().map(|t| t.uuid.as_str()).collect();

        let mut new_performers: HashMap<String, Performer> = HashMap::new();

        for performer in every_performers_in_db.iter_mut() {
            let orig_size = performer.tracks_ids.len();

            performer
                .tracks_ids
                .retain(|t| exists_tracks.contains(t.as_str()));

            // update if need
            if orig_size != performer.tracks_ids.len() && performer.tracks_ids.len() != 0 {
                let first_track = every_tracks_in_db
                    .iter()
                    .find(|t| t.uuid == performer.tracks_ids[0])
                    .unwrap();

                performer.update_performer(first_track);
                db.add_to_db(performer)?;
                debug(format!("{} updated", performer.title.to_string()));
            }

            // remove if empty
            if performer.tracks_ids.len() == 0 {
                debug(format!("{} removed", performer.title.to_string()));
                db.remove_from_db(performer)?;
            }

            // remove tracks, that now in performers
            for i in &performer.tracks_ids {
                exists_tracks.remove(i.as_str());
            }
        }

        for track_uuid in exists_tracks.iter() {
            let track = every_tracks_in_db
                .iter()
                .find(|t| t.uuid == *track_uuid)
                .unwrap();

            if let Some(performer) = new_performers.get_mut(&track.performer) {
                performer.tracks_ids.push(track_uuid.to_string());
            } else if let Some(performer) = every_performers_in_db
                .iter_mut()
                .find(|a| a.title == track.performer)
            {
                performer.tracks_ids.push(track_uuid.to_string());
                performer.update_performer(track);

                db.add_to_db(performer)?;
                debug(format!(
                    "{} updated with new track",
                    performer.title.to_string()
                ));
            } else {
                let performer = Self::create_new_performer(track, vec![track_uuid.to_string()]);
                new_performers.insert(performer.title.clone(), performer);
            }
        }

        for performer in new_performers {
            debug(format!("{} created", performer.0));
            db.add_to_db(&performer.1)?;
        }

        Ok(())
    }

    fn update_performer(&mut self, first_track: &Track) {
        self.title = first_track.performer.clone();
    }

    fn create_new_performer(track: &Track, tracks_ids: Vec<String>) -> Self {
        Self {
            uuid: Uuid::new_v4().to_string(),
            tracks_ids,
            title: track.performer.clone(),
        }
    }

    pub fn edit_metadata(&mut self, db: &DB, title: String) -> Result<()> {
        self.title = title;

        for track_uuid in self.tracks_ids.iter() {
            let track = db
                .get_media_by_id(track_uuid.clone(), MediaType::Track)
                .unwrap()
                .unwrap_as_track();

            let title = track.title.clone();
            let album = track.album.clone();
            let genre = track.genre.clone();
            _ = db
                .edit_track(
                    track,
                    track_uuid.clone(),
                    title,
                    album,
                    self.title.clone(),
                    genre,
                    None,
                )
                .map_err(|err| error(err.to_string()));
        }

        db.update_in_db(self)?;

        Ok(())
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

impl MediaArray for Performer {
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
        let album = Self::create_new_performer(track, vec![track.uuid.clone()]);
        db.add_to_db(&album);
    }
}
