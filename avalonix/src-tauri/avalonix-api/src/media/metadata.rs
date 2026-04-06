#![allow(missing_docs)]

use lofty::config::ParseOptions;
use lofty::prelude::*;
use lofty::probe::Probe;
use rkyv::{Archive, Deserialize, Serialize};
use std::fmt;

use crate::db::MusicDB;
use crate::logger;
use crate::playable::track::Track;
use crate::useful_traits::uri_create::CreateUri;
use crate::utils::get_argument_val;

#[derive(
    ts_rs::TS, serde::Serialize, serde::Deserialize, Archive, Deserialize, Serialize, Debug, Clone,
)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\Metadata.ts")]
pub struct Metadata {
    pub title: Option<String>,
    pub artist: Option<String>,
    pub album: Option<String>,
    pub genre: Option<String>,
    pub year: Option<u16>,
    pub lyrics: Option<String>,
    pub bitrate: Option<u32>,
    pub duration_secs: u64,
    pub track_cover_uri: Option<String>,
}

impl Metadata {
    pub fn from(track_path: &str, db: &MusicDB, tracks_hash: Vec<&Track>) -> Result<Self, String> {
        let mut track_hash = None;

        for track in tracks_hash {
            if track.file_path == track_path {
                track_hash = Some(track.clone());
                break;
            }
        }

        match track_hash {
            Some(hash) => {
                logger::debug(&format!("track metadata {} loaded from hash", track_path));
                Ok(hash.metadata)
            }
            None => {
                let options = ParseOptions::new().parsing_mode(lofty::config::ParsingMode::Relaxed);

                let tagged_file = Probe::open(track_path)
                    .map_err(|err| format!("cant read file {} : {}", track_path, err))?
                    .options(options)
                    .read()
                    .map_err(|err| format!("error when read metadata {}", err))?;

                let properties = tagged_file.properties();
                let tag = match tagged_file.primary_tag() {
                    Some(primary_tag) => primary_tag,
                    None => tagged_file.first_tag().ok_or("ERROR: No tags found!")?,
                };

                let result = Metadata {
                    title: tag.title().map(String::from),
                    artist: tag.artist().map(String::from),
                    album: tag.album().map(String::from),
                    genre: tag.genre().map(String::from),
                    year: tag.date().map(|d| d.year),
                    lyrics: tag.get_string(ItemKey::Lyrics).map(String::from),
                    bitrate: properties.overall_bitrate(),
                    duration_secs: properties.duration().as_secs(),
                    track_cover_uri: None,
                };

                let track = Track::from(track_path, &result);
                db.save_track(&track).unwrap();
                logger::debug(&format!(
                    "track metadata {} loaded without hash",
                    track_path
                ));
                drop(track);
                Ok(result)
            }
        }
    }

    pub fn get_cover(track_path: &str) -> Result<Option<String>, String> {
        let options = ParseOptions::new().parsing_mode(lofty::config::ParsingMode::Relaxed);

        let tagged_file = Probe::open(track_path)
            .map_err(|err| format!("cant read file {} : {}", track_path, err))?
            .options(options)
            .read()
            .map_err(|err| format!("error when read metadata {}", err))?;

        let tag = match tagged_file.primary_tag() {
            Some(primary_tag) => primary_tag,
            None => tagged_file
                .first_tag()
                .ok_or("ERROR: No tags found!")
                .unwrap(),
        };

        let pic = tag.pictures().first();

        match pic {
            Some(pic) => {
                return Ok(Some(pic.create_uri()));
            }
            None => return Ok(None),
        };
    }

    pub fn add_cover_to_metadata(&mut self, track_path: &str) -> Result<(), String> {
        let cover = Self::get_cover(track_path);
        match cover {
            Ok(cover) => self.track_cover_uri = cover,
            Err(_) => self.track_cover_uri = None,
        }
        Ok(())
    }
}

impl fmt::Display for Metadata {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(
            f,
            "\ntitle: {}\n artist: {}\n album: {}\n genre: {}\n year: {}\n lyric: {}\n bitrate: {}\n duration: {}",
            self.title.clone().unwrap_or_default(),
            self.artist.clone().unwrap_or_default(),
            self.album.clone().unwrap_or_default(),
            self.genre.clone().unwrap_or_default(),
            self.year.clone().unwrap_or_default(),
            self.lyrics.clone().unwrap_or_default(),
            self.bitrate.clone().unwrap_or_default(),
            self.duration_secs,
        )
    }
}

#[test]
fn test_metadata_from() {
    use crate::disk_manager;
    use crate::logger;

    let hash_path = disk_manager::avalonix_special_folder_path();

    let music_path = get_argument_val(&"TRACK_PATH");
    let Some(_) = music_path else {
        return;
    };

    let db = MusicDB::open(&hash_path);

    match db {
        Ok(db) => {
            let all_tracks = db.get_all_tracks().unwrap();
            let tracks_hash = all_tracks.iter().collect();
            let metadata = Metadata::from(&music_path.unwrap(), &db, tracks_hash);

            logger::debug(&format!("{}", metadata.unwrap()));
        }
        Err(err) => logger::error(&err.to_string()),
    }
}
