use std::{
    collections::HashMap,
    sync::{Arc, Mutex},
};

use crate::{
    db::MusicDB,
    playboxes::{album::Album, tracks_container::TracksContainer},
};

#[derive(ts_rs::TS)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\AlbumsContainer.ts")]
pub struct AlbumsContainer {
    pub albums: HashMap<String, Arc<Mutex<Album>>>,
}

impl AlbumsContainer {
    /*
    pub fn new(container: &TracksContainer, db: &MusicDB) -> AlbumsContainer {
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
        AlbumsContainer { albums }
    }
     */
}
