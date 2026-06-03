use anyhow::Result;

pub trait Media {
    fn convert_to_db(&self) -> Result<(String, Vec<u8>)>;
}
