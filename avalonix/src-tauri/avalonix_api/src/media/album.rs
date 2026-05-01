use std::{
    collections::{HashMap, HashSet, hash_set},
    fmt::Display,
};

use uuid::Uuid;

use crate::{
    disk::{db::DB, settings::Settings},
    logger,
    media::{album, track::Track, tracks_group::TracksGroup},
    metadata::album_metadata::{self, AlbumMetadata},
};

use rkyv::{Archive, Deserialize, Serialize};

#[derive(Debug, Archive, Serialize, Deserialize, Clone)]
pub struct Album {
    pub tracks_ids: Vec<Vec<u8>>,
    pub album_metadata: AlbumMetadata,
}

impl Album {
    pub fn from(
        tracks_ids: Vec<Vec<u8>>,
        album_metadata: AlbumMetadata,
        albums_hash: &Vec<Self>,
    ) -> Self {
        Self {
            tracks_ids,
            album_metadata,
        }
    }
}

impl Display for Album {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.album_metadata)
    }
}

impl TracksGroup for Album {
    type Output = Self;
    fn group_tracks(
        albums_hash: &Vec<Self::Output>,
        tracks_hash: &Vec<super::track::Track>,
    ) -> anyhow::Result<Vec<Self::Output>> {
        let mut result = vec![];

        let mut groups: HashMap<String, Vec<&Track>> = HashMap::new();

        for track in tracks_hash {
            let track_album_name = &track.metadata.album;
            if let Some(album) = groups.get_mut(track_album_name) {
                album.push(track);
            } else {
                groups.insert(track_album_name.clone(), vec![track]);
            }
        }

        for i in groups {
            let mut ids = vec![];

            for track in &i.1 {
                ids.push(track.metadata.id.clone());
            }

            let id = Uuid::new_v4().to_bytes_le().to_vec();

            let track = i.1[0];
            let album_metadata = AlbumMetadata::from(&id, track, albums_hash);
            let album = Album::from(ids, album_metadata, albums_hash);

            result.push(album);
        }

        Ok(result)
    }
}

#[test]
fn test_albums_grouping() -> anyhow::Result<()> {
    let db = DB::open()?;

    let mut db_guard = db.lock().unwrap();
    db_guard.load_tracks_hash()?;
    db_guard.load_albums_hash()?;

    let tracks_hash = &db_guard.db_hash.tracks_hash;
    let albums_hash = &db_guard.db_hash.albums_hash;

    let albums = Album::group_tracks(albums_hash, tracks_hash)?;
    for album in albums {
        logger::debug(album.album_metadata.album_title);
    }
    Ok(())
}
