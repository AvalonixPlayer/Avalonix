use std::{fs::File, io::Cursor};

pub struct Track {
    pub audio_data: Vec<u8>,
}

impl Track {
    pub fn get_data(&self) -> anyhow::Result<Cursor<Vec<u8>>> {
        let cursor = Cursor::new(self.audio_data.clone());
        Ok(cursor)
    }
}
