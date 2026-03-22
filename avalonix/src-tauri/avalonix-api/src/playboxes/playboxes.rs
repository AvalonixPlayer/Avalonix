use std::{collections::HashMap, sync::Arc};

use crate::{
    db::MusicDB,
    disk_manager, logger,
    media::{
        metadata::Metadata,
        track::{self, Track},
    },
};

#[derive(ts_rs::TS)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\TracksContainer.ts")]
pub struct TracksContainer {
    pub all_tracks: Vec<Arc<Track>>,
}

#[derive(ts_rs::TS)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\AlbumsContainer.ts")]
pub struct AlbumsContainer {
    pub albums: HashMap<String, Vec<Arc<Track>>>,
}

#[derive(ts_rs::TS)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\AristsContainer.ts")]
pub struct AristsContainer {
    pub artists: HashMap<String, Vec<Arc<Track>>>,
}

pub struct PlayboxesManager {
    pub tracks_container: TracksContainer,
    pub albums_container: AlbumsContainer,
    pub artists_container: AristsContainer,
}

impl TracksContainer {
    pub fn new(db: &MusicDB) -> TracksContainer {
        let all_tracks_paths = disk_manager::get_all_tracks_paths();
        let mut result_tracks: Vec<Arc<Track>> = Vec::new();

        for track_path in all_tracks_paths {
            let all_tracks = db.get_all_tracks().unwrap();
            let tracks_hash = all_tracks.iter().collect();

            let track_metadata = Metadata::from(&track_path, &db, tracks_hash);
            match track_metadata {
                Ok(track_metadata) => {
                    result_tracks.push(Arc::new(Track::new(&track_path, track_metadata)))
                }
                Err(err) => logger::error(&err.to_string()),
            }
        }
        TracksContainer {
            all_tracks: result_tracks,
        }
    }
}

impl AlbumsContainer {
    pub fn new(container: &TracksContainer) -> AlbumsContainer {
        let all_tracks = container.all_tracks.clone();

        let mut albums: HashMap<String, Vec<Arc<Track>>> = HashMap::new();

        for track in all_tracks {
            match track.metadata.album.as_ref() {
                Some(album_name) => {
                    let album = albums.get_mut(album_name);
                    match album {
                        Some(album) => album.push(track),
                        None => {
                            let mut vec = Vec::new();
                            vec.push(track.clone());
                            albums.insert(album_name.clone(), vec);
                        }
                    }
                }
                None => match albums.get_mut("Unknown album") {
                    Some(album) => album.push(track),
                    None => {
                        let mut vec = Vec::new();
                        vec.push(track);
                        albums.insert("Unknown album".to_string(), vec);
                    }
                },
            }
        }

        AlbumsContainer { albums }
    }
}

impl AristsContainer {
    pub fn new(container: &TracksContainer) -> AristsContainer {
        let all_tracks = container.all_tracks.clone();

        let mut artists: HashMap<String, Vec<Arc<Track>>> = HashMap::new();

        for track in all_tracks {
            match track.metadata.artist.as_ref() {
                Some(artist_name) => {
                    let artist = artists.get_mut(artist_name);
                    match artist {
                        Some(artist) => artist.push(track),
                        None => {
                            let mut vec = Vec::new();
                            vec.push(track.clone());
                            artists.insert(artist_name.clone(), vec);
                        }
                    }
                }
                None => match artists.get_mut("Unknown artist") {
                    Some(artist) => artist.push(track),
                    None => {
                        let mut vec = Vec::new();
                        vec.push(track);
                        artists.insert("Unknown artist".to_string(), vec);
                    }
                },
            }
        }

        AristsContainer { artists }
    }
}

impl PlayboxesManager {
    pub fn new(
        tracks_container: TracksContainer,
        albums_container: AlbumsContainer,
        artists_container: AristsContainer,
    ) -> PlayboxesManager {
        PlayboxesManager {
            tracks_container,
            albums_container,
            artists_container,
        }
    }
}

#[test]
fn test_track_container_new() {
    let hash_path = disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(&hash_path).unwrap();
    let cont = TracksContainer::new(&db);

    let tracks = &cont.all_tracks;

    for track in tracks {
        logger::debug(&format!("{}", track.metadata));
    }
}

#[test]
fn test_albums_container_new() {
    let hash_path = disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(&hash_path).unwrap();
    let cont = TracksContainer::new(&db);

    let albums_container = AlbumsContainer::new(&cont);

    for album in albums_container.albums {
        for track in album.1 {
            logger::debug(&format!(
                "album - {}; track - {}",
                album.0,
                track.metadata.title.clone().unwrap()
            ));
        }
    }
}

#[test]
fn test_artists_container_new() {
    let hash_path = disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(&hash_path).unwrap();
    let cont = TracksContainer::new(&db);

    let artists_container = AristsContainer::new(&cont);

    for artist in artists_container.artists {
        for track in artist.1 {
            logger::debug(&format!(
                "artist - {}; track - {}",
                artist.0.clone(),
                track.metadata.title.clone().unwrap()
            ));
        }
    }
}
