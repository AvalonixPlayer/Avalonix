use std::{fmt::Display, time::Duration};

use rkyv::{Archive, Deserialize, Serialize};

use crate::disk::db::{self, DB};

#[derive(Debug, Archive, Serialize, Deserialize, Clone)]
pub struct TrackMetadata {
    pub id: Vec<u8>,
    pub file_path: String,
    pub start_pos: Duration,
    pub end_pos: Duration,
    pub title: String,
    pub album: String,
    pub artist: String,
    pub genre: String,
}

impl TrackMetadata {
    pub fn new(
        id: &Vec<u8>,
        file_path: &str,
        start_pos: Duration,
        end_pos: Duration,
        title: &str,
        album: &str,
        artist: &str,
        genre: &str,
    ) -> Self {
        Self {
            id: id.clone(),
            file_path: file_path.to_string(),
            start_pos,
            end_pos,
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
            "\n\t{:?}\n\tfile_path: \"{}\" \n\ttitle: {}\n\talbum: {}\n\tartist: {}\n\tgenre: {}\n\tstart: {}\n\tend: {}",
            self.id,
            self.file_path,
            self.title,
            self.album,
            self.artist,
            self.genre,
            self.start_pos.as_secs(),
            self.end_pos.as_secs()
        )
    }
}

#[test]
pub fn test_parse_metadata_from_file() -> anyhow::Result<()> {
    use crate::{
        logger,
        media::audio_file::{self, AudioFile},
    };

    use crate::utils::get_argument_val;

    let file_path = get_argument_val("TRACK_PATH");
    if file_path == None {
        return Ok(());
    }

    let file_path = file_path.unwrap();

    let db = DB::open()?;

    logger::debug(audio_file::SingleFile::read_metadatas(&file_path, &db)?[0].to_string());

    Ok(())
}

#[test]
pub fn test_parse_metadata_from_cue() -> anyhow::Result<()> {
    use crate::{
        logger,
        media::audio_file::{self, AudioFile},
    };

    use crate::utils::get_argument_val;

    let file_path = get_argument_val("TRACK_PATH");
    if file_path == None {
        return Ok(());
    }

    let file_path = file_path.unwrap();

    let db = DB::open()?;

    for track in audio_file::CUEFile::read_metadatas(&file_path, &db)? {
        logger::debug(track.to_string());
    }

    Ok(())
}
