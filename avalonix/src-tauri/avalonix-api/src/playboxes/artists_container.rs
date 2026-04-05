use std::{
    collections::HashMap,
    sync::{Arc, Mutex},
};

use crate::{media::track::Track, playboxes::tracks_container::TracksContainer};

#[derive(ts_rs::TS)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\AristsContainer.ts")]
pub struct AristsContainer {
    pub artists: HashMap<String, Vec<Arc<Mutex<Track>>>>,
}

impl AristsContainer {
    /*
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
     */
}
