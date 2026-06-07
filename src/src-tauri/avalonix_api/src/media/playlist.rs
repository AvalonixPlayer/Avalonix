use rkyv::{Archive, Deserialize, Serialize};

use crate::media::media_trait::Media;

#[derive(Archive, Deserialize, Serialize)]
pub struct Playlist {}

impl Media for Playlist {
    fn get_media_type(&self) -> super::media_trait::MediaType {
        super::media_trait::MediaType::Playlist
    }
    fn convert_to_db(&self) -> anyhow::Result<(String, Vec<u8>)> {
        Ok((String::new(), vec![]))
    }
}
