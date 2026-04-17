use lofty::{config::ParseOptions, file::TaggedFileExt, probe::Probe, tag::Accessor};

use crate::metadata::track_metadata::TrackMetadata;

pub trait AudioFile {
    fn read_metadatas(file_path: String) -> anyhow::Result<Vec<TrackMetadata>>;
}

pub struct SingleFile {}

pub struct CUEFile {}

impl AudioFile for SingleFile {
    fn read_metadatas(file_path: String) -> anyhow::Result<Vec<TrackMetadata>> {
        let options = ParseOptions::new().parsing_mode(lofty::config::ParsingMode::Relaxed);

        let tagged_file = Probe::open(&file_path)?.options(options).read()?;

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

        Ok(vec![TrackMetadata {
            file_path,
            title,
            album,
            artist,
            genre,
        }])
    }
}

impl AudioFile for CUEFile {
    fn read_metadatas(file_path: String) -> anyhow::Result<Vec<TrackMetadata>> {
        let result_vec = vec![];

        Ok(result_vec)
    }
}
