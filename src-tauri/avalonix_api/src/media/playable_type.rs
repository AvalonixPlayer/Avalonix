use serde::{Deserialize, Serialize};
use ts_rs::TS;

use crate::media::{album::Album, performer::Performer, playlist::Playlist, track::Track};

#[derive(TS, Deserialize)]
#[ts(export)]
pub enum MediaType {
    Track,
    Album,
    Performer,
    Playlist,
}

#[derive(TS, Serialize, Deserialize)]
#[serde(tag = "type", content = "data")]
#[ts(export)]
pub enum PlayableResult {
    Track(Track),
    Album(Album),
    Performer(Performer),
    Playlist(Playlist),
}

impl PlayableResult {
    pub fn unwrap_as_track(self) -> Track {
        match self {
            Self::Track(track) => track,
            _ => unreachable!(),
        }
    }

    pub fn unwrap_as_album(self) -> Album {
        match self {
            Self::Album(album) => album,
            _ => unreachable!(),
        }
    }

    pub fn unwrap_as_performer(self) -> Performer {
        match self {
            Self::Performer(performer) => performer,
            _ => unreachable!(),
        }
    }

    pub fn unwrap_as_playlist(self) -> Playlist {
        match self {
            Self::Playlist(playylist) => playylist,
            _ => unreachable!(),
        }
    }
}
