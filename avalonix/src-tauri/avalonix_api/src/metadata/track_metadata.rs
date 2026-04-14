use std::fmt::Display;

use anyhow::Ok;
use lofty::{config::ParseOptions, file::TaggedFileExt, probe::Probe, tag::Accessor};

use crate::logger;

pub struct TrackMetadata {
    pub title: String,
    pub album: String,
    pub artist: String,
    pub genre: String,
}

impl TrackMetadata {
    pub fn read(file_path: &str) -> anyhow::Result<Self> {
        let options = ParseOptions::new().parsing_mode(lofty::config::ParsingMode::Relaxed);

        let tagged_file = Probe::open(file_path)?.options(options).read()?;

        let tag = match tagged_file.primary_tag() {
            Some(primary_tag) => primary_tag,
            None => tagged_file.first_tag().expect("ERROR: No tags found!"),
        };

        let title = tag
            .title()
            .as_deref()
            .map_or("Unknown title", |v| v)
            .to_string();
        let album = tag
            .album()
            .as_deref()
            .map_or("Unknown album", |v| v)
            .to_string();
        let artist = tag
            .artist()
            .as_deref()
            .map_or("Unknown artist", |v| v)
            .to_string();
        let genre = tag
            .genre()
            .as_deref()
            .map_or("Unknown genre", |v| v)
            .to_string();

        Ok(Self {
            title,
            album,
            artist,
            genre,
        })
    }
}

impl Display for TrackMetadata {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "title: {}\n album: {}\n artist: {}\n genre: {}\n",
            self.title, self.album, self.artist, self.genre
        )
    }
}

#[test]
pub fn test_track_metadata() -> anyhow::Result<()> {
    use crate::utils::get_argument_val;

    let path = get_argument_val("TRACK_PATH");
    if path == None {
        return Ok(());
    }

    let path = path.unwrap();

    let track_metadata = TrackMetadata::read(&path)?;

    logger::debug(track_metadata);

    Ok(())
}
