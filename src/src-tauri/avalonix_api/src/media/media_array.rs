use crate::{disk::db::DB, media::track::Track};

pub trait MediaArray {
    fn add_track(&mut self, db: &DB, track_uuid: String);
    fn remove_track(&mut self, db: &DB, track_uuid: String);
    fn create_new_for_track(db: &DB, track: &Track);
}

pub enum MediaArrayType {
    Album,
    Performer,
    Playlist,
}
