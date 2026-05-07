use std::{collections::HashMap, fmt::Display};

use uuid::Uuid;

use crate::{
    media::{track::Track, tracks_group::TracksGroup},
    metadata::album_metadata::AlbumMetadata,
};

use rkyv::{Archive, Deserialize, Serialize};

/// Album structure
#[derive(Debug, Archive, Serialize, Deserialize, Clone)]
pub struct Album {
    /// List of album track IDs
    pub tracks_ids: Vec<Vec<u8>>,
    /// Album metadata
    pub album_metadata: AlbumMetadata,
}

impl Album {
    fn from(
        tracks_ids: Vec<Vec<u8>>,
        album_metadata: AlbumMetadata,
        _albums_hash: &Vec<Self>,
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
