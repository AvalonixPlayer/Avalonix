use std::{fmt, sync::Arc};

use crate::{
    db::MusicDB, disk_manager, logger, media::track::Track, playboxes::playboxes::TracksContainer,
};

#[derive(Debug)]
pub struct PlayQueue {
    pub tracks: Vec<Arc<Track>>,
}

impl PlayQueue {
    pub fn new() -> Self {
        PlayQueue { tracks: Vec::new() }
    }

    pub fn clear(&mut self) {
        self.tracks.clear();
    }

    pub fn add_track(&mut self, track: Arc<Track>) {
        self.tracks.push(track);
    }

    pub fn add_tracks(&mut self, tracks: Vec<Arc<Track>>) {
        for track in tracks {
            self.tracks.push(track);
        }
    }

    pub fn remove_track(&mut self, track: Arc<Track>) {
        let mut index = 0;
        for i in self.tracks.clone() {
            if i == track {
                self.tracks.remove(index);
                break;
            }
            index += 1;
        }
    }
}

#[test]
fn test_play_queue() {
    let hash_path = disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(&hash_path).unwrap();
    let cont = TracksContainer::new(&db);

    let mut queue = PlayQueue::new();

    let track = cont.all_tracks[0].clone();
    queue.add_track(track.clone());

    println!("{:#?}", queue);

    queue.remove_track(track.clone());

    logger::debug(&format!("{:#?}", queue));
}
