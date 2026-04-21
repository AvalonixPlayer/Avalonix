use std::{
    path::{Path, PathBuf},
    time::Duration,
};

use anyhow::bail;
use lofty::{
    config::ParseOptions,
    file::{AudioFile as LoftyAudioFile, TaggedFileExt},
    probe::Probe,
    tag::Accessor,
};
use rcue::parser::parse_from_file;

use crate::{logger, metadata::track_metadata::TrackMetadata};

pub trait AudioFile {
    fn read_metadatas<P: AsRef<Path>>(file_path: P) -> anyhow::Result<Vec<TrackMetadata>>;
}

pub struct SingleFile;

pub struct CUEFile;

impl AudioFile for SingleFile {
    fn read_metadatas<P: AsRef<Path>>(path: P) -> anyhow::Result<Vec<TrackMetadata>> {
        let options = ParseOptions::new().parsing_mode(lofty::config::ParsingMode::Relaxed);

        let tagged_file = Probe::open(&path)?.options(options).read()?;

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
            file_path: path.as_ref().to_str().unwrap().to_string(),
            start_pos: Duration::new(0, 0),
            end_pos: dur,
            title,
            album,
            artist,
            genre,
        }])
    }
}

pub enum LibFile {
    Audio,
    Cue,
    NotForLib,
}

pub trait LibFileTrait {
    fn file_type(&self) -> LibFile;
}

impl<P: AsRef<Path>> LibFileTrait for P {
    fn file_type(&self) -> LibFile {
        match infer::get_from_path(&self) {
            Ok(t) => match t {
                Some(t) => match t.matcher_type() {
                    infer::MatcherType::Audio => LibFile::Audio,
                    infer::MatcherType::Text => LibFile::Cue,
                    _ => LibFile::NotForLib,
                },
                None => match CUEFile::check_cue(&self) {
                    Ok(_) => LibFile::Cue,
                    Err(_) => {
                        logger::info(format!(
                            "file is not for lib: {}",
                            self.as_ref().to_str().unwrap()
                        ));
                        LibFile::NotForLib
                    }
                },
            },
            Err(_) => {
                logger::error(format!(
                    "file can`t be read: {}",
                    self.as_ref().to_str().unwrap()
                ));
                LibFile::NotForLib
            }
        }
    }
}

impl AudioFile for CUEFile {
    fn read_metadatas<P: AsRef<Path>>(file_path: P) -> anyhow::Result<Vec<TrackMetadata>> {
        let mut result_vec = vec![];

        let cue = parse_from_file(&file_path.as_ref().to_str().unwrap().to_string(), false)?;

        let cue_title = cue.title;
        let cue_performer = cue.performer;

        match (cue_title, cue_performer) {
            (Some(cue_title), Some(cue_performer)) => {
                let cue_files = cue.files;

                for file in cue_files {
                    for (i, track) in file.tracks.iter().enumerate() {
                        let fp = file_path.as_ref().parent().unwrap().join(&file.file);

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
                bail!(format!(
                    "can`t to read cue: {}",
                    file_path.as_ref().to_str().unwrap()
                ))
            }
        }

        Ok(result_vec)
    }
}

impl CUEFile {
    pub fn check_cue<P: AsRef<Path>>(file_path: P) -> anyhow::Result<()> {
        let _ = parse_from_file(&file_path.as_ref().to_str().unwrap().to_string(), false)?;
        Ok(())
    }
}
