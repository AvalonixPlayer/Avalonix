use anyhow::Result;

pub enum MediaType {
    Track,
    Album,
    Performer,
    Playlist,
}

pub trait Media {
    fn get_media_type(&self) -> MediaType;
    fn convert_to_db(&self) -> Result<(String, Vec<u8>)>;
}
