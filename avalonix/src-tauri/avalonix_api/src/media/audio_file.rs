use std::{path::PathBuf, time::Duration};

use anyhow::bail;
use lofty::{
    config::ParseOptions,
    file::{AudioFile as LoftyAudioFile, TaggedFileExt},
    probe::Probe,
    tag::Accessor,
};
use rcue::parser::parse_from_file;

use crate::metadata::track_metadata::TrackMetadata;

pub trait AudioFile {
    fn read_metadatas(file_path: &str) -> anyhow::Result<Vec<TrackMetadata>>;
}

pub struct SingleFile;

pub struct CUEFile;

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

        let properties = tagged_file.properties();
        let dur = properties.duration();

        Ok(vec![TrackMetadata {
            file_path: file_path.to_string(),
            start_pos: Duration::new(0, 0),
            end_pos: dur,
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
                    for (i, track) in file.tracks.iter().enumerate() {
                        let fp = PathBuf::from(file_path).parent().unwrap().join(&file.file);

                        let title = track.title.as_ref().map_or("Unknown title", |f| f);
                        let artist = &cue_performer;
                        let album = &cue_title;

                        let start_pos = track.indices.get(0).map_or(Duration::new(0, 0), |f| f.1);
                        let end_pos = file.tracks.get(i + 1).map_or(
                            {
                                let options = ParseOptions::new()
                                    .parsing_mode(lofty::config::ParsingMode::Relaxed);
                                let tagged_file = Probe::open(&fp)?.options(options).read()?;
                                tagged_file.properties().duration()
                            },
                            |f| f.indices.get(0).unwrap().1,
                        );

                        let tm = TrackMetadata::new(
                            &fp.to_str().unwrap().to_string(),
                            start_pos,
                            end_pos,
                            title,
                            album,
                            artist,
                            "Unknown genre",
                        );
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
