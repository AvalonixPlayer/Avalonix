use std::collections::HashMap;

use crate::{
    db::MusicDB,
    disk_manager, logger,
    media::{
        metadata::Metadata,
        track::{self, Track},
    },
};

pub struct TracksContainer {
    pub all_tracks: Vec<Track>,
}

pub struct AlbumsContainer<'a> {
    pub albums: HashMap<String, Vec<&'a Track>>,
}

impl TracksContainer {
    pub fn new(db: &MusicDB) -> TracksContainer {
        let all_tracks_paths = disk_manager::get_all_tracks_paths();
        let mut result_tracks: Vec<Track> = Vec::new();

        for track_path in all_tracks_paths {
            let track_metadata = Metadata::from(&track_path, &db);
            match track_metadata {
                Ok(track_metadata) => result_tracks.push(Track::new(&track_path, track_metadata)),
                Err(err) => logger::error(&err.to_string()),
            }
        }
        TracksContainer {
            all_tracks: result_tracks,
        }
    }
}

impl<'a> AlbumsContainer<'a> {
    pub fn new(container: &'a TracksContainer) -> AlbumsContainer<'a> {
        let all_tracks: Vec<&Track> = container.all_tracks.iter().collect();

        let mut albums: HashMap<String, Vec<&'a Track>> = HashMap::new();

        for track in all_tracks {
            match track.metadata.album.as_ref() {
                Some(album_name) => {
                    let album = albums.get_mut(album_name);
                    match album {
                        Some(album) => album.push(track),
                        None => {
                            let mut vec = Vec::new();
                            vec.push(track);
                            albums.insert(album_name.clone(), vec);
                        }
                    }
                }
                None => {}
            }
        }

        AlbumsContainer { albums }
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
