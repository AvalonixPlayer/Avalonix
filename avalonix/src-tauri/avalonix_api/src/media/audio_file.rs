use std::{path::Path, time::Duration};

use anyhow::bail;
use lofty::{
    config::ParseOptions,
    file::{AudioFile as LoftyAudioFile, TaggedFileExt},
    probe::Probe,
    properties,
    tag::Accessor,
};
use rcue::parser::parse_from_file;
use rustc_serialize::base64::{MIME, ToBase64};
use uuid::Uuid;

use crate::{disk::db::DB, logger, metadata::track_metadata::TrackMetadata};

pub trait AudioFile {
    fn read_metadatas<P: AsRef<Path>>(file_path: P, db: &DB) -> anyhow::Result<Vec<TrackMetadata>>;

    fn get_cover_as_uri<P: AsRef<Path>>(path: P) -> anyhow::Result<String>;
}

pub struct SingleFile;

pub struct CUEFile;

impl AudioFile for SingleFile {
    fn read_metadatas<P: AsRef<Path>>(path: P, db: &DB) -> anyhow::Result<Vec<TrackMetadata>> {
        let options = ParseOptions::new().parsing_mode(lofty::config::ParsingMode::Relaxed);

        let tagged_file = Probe::open(&path)?.options(options).read()?;

        if let Some(track) = db
            .db_hash
            .tracks_hash
            .iter()
            .find(|track| track.metadata.file_path == path.as_ref().to_str().unwrap())
        {
            logger::info(format!(
                "Single file track loaded from hash: {}",
                path.as_ref().to_str().unwrap()
            ));
            return Ok(vec![track.metadata.clone()]);
        }

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
        let bitrate = properties.overall_bitrate().map_or(0, |f| f);

        logger::info(format!(
            "Single file track loaded without hash: {}",
            path.as_ref().to_str().unwrap()
        ));

        Ok(vec![TrackMetadata {
            id: Uuid::new_v4().to_bytes_le().to_vec(),
            file_path: path.as_ref().to_str().unwrap().to_string(),
            start_pos: Duration::new(0, 0),
            end_pos: dur,
            title,
            album,
            artist,
            genre,
            bitrate,
        }])
    }

    fn get_cover_as_uri<P: AsRef<Path>>(path: P) -> anyhow::Result<String> {
        let options = ParseOptions::new().parsing_mode(lofty::config::ParsingMode::Relaxed);

        let tagged_file = Probe::open(&path)?.options(options).read()?;

        if let Some(tag) = tagged_file.primary_tag().or(tagged_file.first_tag()) {
            if let Some(picture) = tag.pictures().first() {
                let data = picture.data();

                let bs64 = data.to_base64(MIME);
                let string = format!("data:image/jpg;base64,{}", bs64);
                println!("{}", string);
                return Ok(string);
            }
        }
        Ok("".to_string())
    }
}

impl AudioFile for CUEFile {
    fn read_metadatas<P: AsRef<Path>>(file_path: P, db: &DB) -> anyhow::Result<Vec<TrackMetadata>> {
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

                        if let Some(track) = db
                            .db_hash
                            .tracks_hash
                            .iter()
                            .find(|track| track.metadata.file_path == fp)
                        {
                            result_vec.push(track.metadata.clone());
                            continue;
                        }

                        let options =
                            ParseOptions::new().parsing_mode(lofty::config::ParsingMode::Relaxed);
                        let tagged_file = Probe::open(&fp)?.options(options).read()?;

                        let title = track.title.as_ref().map_or("Unknown title", |f| f);
                        let artist = &cue_performer;
                        let album = &cue_title;

                        let start_pos = track.indices.get(0).map_or(Duration::new(0, 0), |f| f.1);
                        let end_pos = file
                            .tracks
                            .get(i + 1)
                            .map_or(tagged_file.properties().duration(), |f| {
                                f.indices.get(0).unwrap().1
                            });

                        let id = Uuid::new_v4().to_bytes_le().to_vec();

                        let bitrate = tagged_file.properties().overall_bitrate().map_or(0, |f| f);

                        let tm = TrackMetadata::new(
                            &id,
                            &fp.to_str().unwrap().to_string(),
                            start_pos,
                            end_pos,
                            title,
                            album,
                            artist,
                            "Unknown genre",
                            bitrate,
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

    fn get_cover_as_uri<P: AsRef<Path>>(path: P) -> anyhow::Result<String> {
        // not realizated now
        Ok("".to_string())
    }
}

impl CUEFile {
    pub fn check_cue<P: AsRef<Path>>(file_path: P) -> anyhow::Result<()> {
        let _ = parse_from_file(&file_path.as_ref().to_str().unwrap().to_string(), false)?;
        Ok(())
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
