use serde::{Deserialize, Serialize};
use ts_rs::TS;

use crate::media::{album::Album, performer::Performer, track::Track};

#[derive(TS, Deserialize)]
#[ts(export)]
pub enum MediaType {
    Track,
    Album,
    Performer,
    Playlist,
}

#[derive(TS, Serialize)]
#[serde(tag = "type", content = "data")]
#[ts(export)]
pub enum PlayableResult {
    Track(Track),
    Album(Album),
    Performer(Performer),
}
