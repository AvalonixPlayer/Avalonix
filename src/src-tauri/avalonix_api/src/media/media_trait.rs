use anyhow::Result;

use crate::{disk::db::DB, media::playable_type::MediaType};

pub trait Media {
    fn get_media_type(&self) -> MediaType;
    fn name_starts_with<P: AsRef<str>>(&self, start: P) -> bool;
    fn convert_to_db(&self) -> Result<(Vec<u8>, Vec<u8>)>;
    fn get_tracks_uuids(&self) -> Vec<String>;
    fn edit_media(&self, uuid: String, db: &DB) -> Result<()>;
}
