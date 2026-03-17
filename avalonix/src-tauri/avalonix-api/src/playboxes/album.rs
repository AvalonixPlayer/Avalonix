use crate::{
    db::MusicDB, disk_manager, logger, media::track::Track, playboxes::playbox::TracksContainer,
};

pub struct Album<'a> {
    pub tracks: Vec<&'a Track>,
}

impl<'a> Album<'a> {
    pub fn new(all_tracks: &'a Vec<Track>) -> Album<'a> {
        let mut tracks: Vec<&'a Track> = Vec::new();

        for i in 0..4 {
            let link = &all_tracks[i];
            tracks.push(link);
        }

        Album { tracks }
    }
}

#[test]
fn test_album_new() {
    let hash_dir = disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(&hash_dir).unwrap();
    let track_cont = TracksContainer::new(&db);
    let album = Album::new(&track_cont.all_tracks);

    for track in album.tracks {
        logger::debug(&track.metadata.title.as_ref().unwrap());
    }
}
