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

impl TracksContainer {
    pub fn new(db: &MusicDB) -> TracksContainer {
        let all_tracks_paths = disk_manager::get_all_tracks_paths();
        let mut result_tracks: Vec<Track> = Vec::new();

        for track_path in all_tracks_paths {
            let track_metadata = Metadata::from(&track_path, &db);
            match track_metadata {
                Ok(track_metadata) => {
                    let track = Track::new(&track_path, track_metadata);
                    result_tracks.push(track);
                }
                Err(err) => logger::error(&err.to_string()),
            }
        }
        TracksContainer {
            all_tracks: result_tracks,
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
