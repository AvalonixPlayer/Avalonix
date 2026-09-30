use rkyv::{Archive, Deserialize, Serialize};
use ts_rs::TS;

#[derive(Archive, Deserialize, Serialize, serde::Serialize, serde::Deserialize, TS, Clone)]
#[ts(export)]
pub enum SortBy {
    SortTracks(SortTracks),
    SortAlbums(SortAlbums),
    SortPerformers(SortPerformers),
    SortPlaylists(SortPlaylists),
}

#[derive(Archive, Deserialize, Serialize, serde::Serialize, serde::Deserialize, TS, Clone)]
#[ts(export)]
pub enum SortTracks {
    ByName,
    ByPerformer,
    ByGenre,
}

#[derive(Archive, Deserialize, Serialize, serde::Serialize, serde::Deserialize, TS, Clone)]
#[ts(export)]
pub enum SortAlbums {
    ByName,
    ByPerformer,
    ByLenght,
}

#[derive(Archive, Deserialize, Serialize, serde::Serialize, serde::Deserialize, TS, Clone)]
#[ts(export)]
pub enum SortPerformers {
    ByName,
    ByLenght,
}

#[derive(Archive, Deserialize, Serialize, serde::Serialize, serde::Deserialize, TS, Clone)]
#[ts(export)]
pub enum SortPlaylists {
    ByName,
    ByLenght,
}
