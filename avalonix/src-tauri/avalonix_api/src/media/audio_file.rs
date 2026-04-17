use anyhow::bail;
use lofty::{config::ParseOptions, file::TaggedFileExt, probe::Probe, tag::Accessor};
use rcue::parser::parse_from_file;

use crate::metadata::track_metadata::TrackMetadata;

pub trait AudioFile {
    fn read_metadatas(file_path: &str) -> anyhow::Result<Vec<TrackMetadata>>;
}

pub struct SingleFile {}

pub struct CUEFile {}

impl AudioFile for SingleFile {
    fn read_metadatas(file_path: &str) -> anyhow::Result<Vec<TrackMetadata>> {
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

        Ok(vec![TrackMetadata {
            file_path: file_path.to_string(),
            title,
            album,
            artist,
            genre,
        }])
    }
}

impl AudioFile for CUEFile {
    fn read_metadatas(file_path: &str) -> anyhow::Result<Vec<TrackMetadata>> {
        let mut result_vec = vec![];

        let cue = parse_from_file(file_path, true)?;

        let cue_title = cue.title;
        let cue_performer = cue.performer;

        match (cue_title, cue_performer) {
            (Some(cue_title), Some(cue_performer)) => {
                let cue_files = cue.files;

                for file in cue_files {
                    for track in file.tracks {
                        let fp = &file.file;

                        let title = track.title.as_ref().map_or("Unknown title", |f| f);
                        let artist = &cue_performer;
                        let album = &cue_title;

                        let tm = TrackMetadata::new(&fp, title, album, artist, "Unknown genre");
                        result_vec.push(tm);
                    }
                }
            }
            _ => {
                bail!(format!("can`t to read cue: {}", file_path))
            }
        }

        Ok(result_vec)
    }
}
