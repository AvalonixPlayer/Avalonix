use std::fmt::Display;

use lofty::{config::ParseOptions, file::TaggedFileExt, probe::Probe, tag::Accessor};

use crate::media::audio_file::{self, AudioFile};

pub struct TrackMetadata {
    pub file_path: String,
    pub title: String,
    pub album: String,
    pub artist: String,
    pub genre: String,
}

impl TrackMetadata {
    pub fn new(file_path: &str, title: &str, album: &str, artist: &str, genre: &str) -> Self {
        Self {
            file_path: file_path.to_string(),
            title: title.to_string(),
            album: album.to_string(),
            artist: artist.to_string(),
            genre: genre.to_string(),
        }
    }
}

impl Display for TrackMetadata {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "\n\ttrack-metadata: \"{}\" \n\ttitle: {}\n\talbum: {}\n\tartist: {}\n\tgenre: {}",
            self.file_path, self.title, self.album, self.artist, self.genre
        )
    }
}

#[test]
pub fn test_parse_metadata_from_file() -> anyhow::Result<()> {
    use crate::logger;

    use crate::utils::get_argument_val;

    let file_path = get_argument_val("TRACK_PATH");
    if file_path == None {
        return Ok(());
    }

    let file_path = file_path.unwrap();

    if let Ok(audio_file) = audio_file::SingleFile::read_metadatas(&file_path) {
        logger::debug(audio_file[0].to_string());
    }

    Ok(())
}

#[test]
pub fn test_parse_metadata_from_cue() -> anyhow::Result<()> {
    use crate::logger;

    use crate::utils::get_argument_val;

    let file_path = get_argument_val("TRACK_PATH");
    if file_path == None {
        return Ok(());
    }

    let file_path = file_path.unwrap();

    if let Ok(audio_file) = audio_file::CUEFile::read_metadatas(&file_path) {
        logger::debug(audio_file[0].to_string());
    }

    Ok(())
}
