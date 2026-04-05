use std::{
    collections::HashMap,
    sync::{Arc, Mutex},
    time::Instant,
};

use crate::{db::MusicDB, disk_manager, logger, media::track::Track, playboxes::album::Album};

#[derive(ts_rs::TS)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\TracksContainer.ts")]
pub struct TracksContainer {
    pub all_tracks: Vec<Arc<Mutex<Track>>>,
}

#[derive(ts_rs::TS)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\AlbumsContainer.ts")]
pub struct AlbumsContainer {
    pub albums: HashMap<String, Arc<Mutex<Album>>>,
}

#[derive(ts_rs::TS)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\AristsContainer.ts")]
pub struct AristsContainer {
    pub artists: HashMap<String, Vec<Arc<Mutex<Track>>>>,
}

pub struct PlayboxesManager {
    pub tracks_container: TracksContainer,
    pub albums_container: AlbumsContainer,
    pub artists_container: AristsContainer,
}

impl TracksContainer {
    pub fn new(db: &MusicDB) -> TracksContainer {
        let all_tracks_paths = disk_manager::get_all_tracks_paths();
        let mut result_tracks: Vec<Arc<Mutex<Track>>> = Vec::new();

        for track_path in all_tracks_paths {
            let all_tracks = db.get_all_tracks().unwrap();
            let tracks_hash = all_tracks.iter().collect();

            let track = Track::new(&track_path, db, tracks_hash);
            match track {
                Ok(track) => result_tracks.push(Arc::new(Mutex::new(track))),
                Err(err) => logger::error(&err.to_string()),
            }
        }
        TracksContainer {
            all_tracks: result_tracks,
        }
    }
}

impl AlbumsContainer {
    pub fn new(container: &TracksContainer, db: &MusicDB) -> AlbumsContainer {
        let start = Instant::now();
        let all_tracks = container.all_tracks.clone();
        let mut albums: HashMap<String, Arc<Mutex<Album>>> = HashMap::new();

        let all_albums = db.get_all_albums().unwrap();
        let all_albums_hash: Vec<&Album> = all_albums.iter().collect();

        for track in all_tracks {
            let album_name = {
                let track_guard = track.lock().unwrap();
                track_guard.metadata.album.clone()
            };

            let key = album_name.unwrap_or_else(|| "Unknown album".to_string());

            if let Some(album_arc) = albums.get(&key) {
                let mut guard = album_arc.lock().unwrap();
                guard.tracks.push(track);
            } else {
                let new_album = Album::from(vec![track]);
                albums.insert(key, Arc::new(Mutex::new(new_album)));
            }
        }

        for i in &albums {
            let album = i.1;
            let mut album_guard = album.lock().unwrap();
            album_guard.load_metadata(db, &all_albums_hash, i.0);
        }

        logger::debug(&format!(
            "albums get: {} milis",
            start.elapsed().as_millis()
        ));
        AlbumsContainer { albums }
    }
}

impl AristsContainer {
    pub fn new(container: &TracksContainer) -> AristsContainer {
        let all_tracks = container.all_tracks.clone();

        let mut artists: HashMap<String, Vec<Arc<Mutex<Track>>>> = HashMap::new();

        for track in all_tracks {
            let track_clone = track.clone();
            let track_guard = track_clone.lock().unwrap();

            match &track_guard.metadata.artist {
                Some(artist_name) => {
                    let artist = artists.get_mut(artist_name);
                    match artist {
                        Some(album) => album.push(track),
                        None => {
                            let mut vec = Vec::new();
                            vec.push(track);
                            artists.insert(artist_name.clone(), vec);
                        }
                    }
                }
                None => match artists.get_mut("Unknown artist") {
                    Some(album) => album.push(track),
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
        logger::debug(&format!("{}", track.lock().unwrap().metadata));
    }
}

/*
#[test]
fn test_albums_container_new() {
    let hash_path = disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(&hash_path).unwrap();
    let cont = TracksContainer::new(&db);

    let albums_container = AlbumsContainer::new(&cont);

    for album in albums_container.albums {
        let album_guard = album.1.lock().unwrap();
        let tracks = album_guard.tracks.clone();
        for track in tracks {
            logger::debug(&format!(
                "album - {}; track - {}",
                album.0,
                track.lock().unwrap().metadata.title.clone().unwrap()
            ));
        }
    }
}
 */

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
                track.lock().unwrap().metadata.title.clone().unwrap()
            ));
        }
    }
}
