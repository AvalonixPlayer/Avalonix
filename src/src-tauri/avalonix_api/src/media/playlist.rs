use rkyv::{Archive, Deserialize, Serialize};

use crate::media::{media_trait::Media, playable_type::MediaType};

#[derive(Archive, Deserialize, Serialize)]
pub struct Playlist {}

impl Media for Playlist {
    fn get_media_type(&self) -> MediaType {
        MediaType::Playlist
    }
    fn convert_to_db(&self) -> anyhow::Result<(String, Vec<u8>)> {
        Ok((String::new(), vec![]))
    }
}
