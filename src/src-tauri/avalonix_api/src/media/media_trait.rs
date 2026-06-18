use anyhow::Result;

use crate::media::playable_type::MediaType;

pub trait Media {
    fn get_media_type(&self) -> MediaType;
    fn convert_to_db(&self) -> Result<(Vec<u8>, Vec<u8>)>;
    fn get_tracks_uuids(&self) -> Vec<String>;
}
