use std::{collections::HashMap, fmt::Display};

use uuid::Uuid;

use crate::{
    media::{track::Track, tracks_group::TracksGroup},
    metadata::performer_metadata::PerformerMetadata,
};

use rkyv::{Archive, Deserialize, Serialize};

#[derive(Debug, Archive, Serialize, Deserialize, Clone)]
pub struct Performer {
    pub tracks_ids: Vec<Vec<u8>>,
    pub performer_metadata: PerformerMetadata,
}

impl Performer {
    pub fn from(
        tracks_ids: Vec<Vec<u8>>,
        performer_metadata: PerformerMetadata,
        _albums_hash: &Vec<Self>,
    ) -> Self {
        Self {
            tracks_ids,
            performer_metadata,
        }
    }
}

impl Display for Performer {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.performer_metadata)
    }
}

impl TracksGroup for Performer {
    type Output = Self;
    fn group_tracks(
        performers_hash: &Vec<Self::Output>,
        tracks_hash: &Vec<super::track::Track>,
    ) -> anyhow::Result<Vec<Self::Output>> {
        let mut result = vec![];

        let mut groups: HashMap<String, Vec<&Track>> = HashMap::new();

        for track in tracks_hash {
            let track_performer_name = &track.metadata.artist;
            if let Some(performer) = groups.get_mut(track_performer_name) {
                performer.push(track);
            } else {
                groups.insert(track_performer_name.clone(), vec![track]);
            }
        }

        for i in groups {
            let mut ids = vec![];

            for track in &i.1 {
                ids.push(track.metadata.id.clone());
            }

            let id = Uuid::new_v4().to_bytes_le().to_vec();

            let track = i.1[0];
            let performer_metadata = PerformerMetadata::from(&id, track, performers_hash);
            let performer = Performer::from(ids, performer_metadata, performers_hash);

            result.push(performer);
        }

        Ok(result)
    }
}

#[test]
fn test_performer_grouping() -> anyhow::Result<()> {
    let db = DB::open()?;

    let mut db_guard = db.lock().unwrap();
    db_guard.load_tracks_hash()?;
    db_guard.load_albums_hash()?;

    let tracks_hash = &db_guard.db_hash.tracks_hash;
    let albums_hash = &db_guard.db_hash.albums_hash;
    let performers = &db_guard.db_hash.albums_hash;

    let performers = Album::group_tracks(performers, tracks_hash)?;
    for performer in performers {
        logger::debug(performer.album_metadata.album_title);
    }
    Ok(())
}
