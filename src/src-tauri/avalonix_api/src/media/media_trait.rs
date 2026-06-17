use anyhow::Result;

use crate::media::playable_type::MediaType;

pub trait Media {
    fn get_media_type(&self) -> MediaType;
    fn convert_to_db(&self) -> Result<(String, Vec<u8>)>;
}
