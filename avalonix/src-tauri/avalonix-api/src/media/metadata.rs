#![allow(missing_docs)]
use lofty::prelude::*;
use lofty::probe::Probe;
use rkyv::{Archive, Deserialize, Serialize};
use std::path::{Path, PathBuf};
use std::{fmt, fs};

use crate::db::MusicDB;
use crate::media::track::Track;
use crate::{disk_manager, logger};

#[derive(ts_rs::TS, serde::Serialize, Archive, Deserialize, Serialize, Debug, Clone)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\Metadata.ts")]
pub struct Metadata {
    pub title: Option<String>,
    pub artist: Option<String>,
    pub album: Option<String>,
    pub genre: Option<String>,
    pub year: Option<u16>,
    pub lyrics: Option<String>,
    pub bitrate: Option<u32>,
    pub album_cover_hash_path: Option<String>,
    pub duration_secs: u64, // u64 instead Duration
}

impl Metadata {
    pub fn from(track_path: &str, db: &MusicDB, tracks_hash: Vec<&Track>) -> Result<Self, String> {
        let path = Path::new(&track_path);

        let mut track_hash = None;

        for track in tracks_hash {
            if track.file_path == track_path {
                track_hash = Some(track.clone());
                break;
            }
        }

        match track_hash {
            Some(hash) => {
                logger::debug("metadata loaded from hash");
                Ok(hash.metadata)
            }
            None => {
                let tagged_file = Probe::open(path)
                    .map_err(|e| format!("ERROR: Bad path: {}", e))?
                    .read()
                    .map_err(|e| {
                        format!(
                            "ERROR: Failed to read file \"{}\": {}",
                            path.to_str().unwrap(),
                            e
                        )
                    })?;

                let properties = tagged_file.properties();
                let tag = match tagged_file.primary_tag() {
                    Some(primary_tag) => primary_tag,
                    None => tagged_file.first_tag().ok_or("ERROR: No tags found!")?,
                };

                let album_name = tag.album().map(String::from);
                let artist_name = tag.artist().map(String::from);

                let album_cover_hash_path: Option<String>;

                match (artist_name, album_name) {
                    (Some(artist_name), Some(album_name)) => {
                        let path = PathBuf::from(disk_manager::avalonix_special_folder_path())
                            .join(format!("{}-{}.jpg", artist_name, album_name))
                            .to_str()
                            .unwrap()
                            .to_string();

                        if fs::exists(&path).unwrap() {
                            album_cover_hash_path = Some(path);
                        } else {
                            let cover_opt = tag.pictures().first();

                            match cover_opt {
                                Some(v) => {
                                    _ = fs::write(&path, v.data().to_owned());
                                    album_cover_hash_path = Some(path);
                                }
                                None => album_cover_hash_path = None,
                            }
                        }
                    }
                    _ => album_cover_hash_path = None,
                }

                let result = Metadata {
                    title: tag.title().map(String::from),
                    artist: tag.artist().map(String::from),
                    album: tag.album().map(String::from),
                    genre: tag.genre().map(String::from),
                    year: tag.date().map(|d| d.year),
                    lyrics: tag.get_string(ItemKey::Lyrics).map(String::from),
                    bitrate: properties.overall_bitrate(),
                    album_cover_hash_path,
                    duration_secs: properties.duration().as_secs(),
                };

                let track = Track::new(track_path, result.clone());
                db.save_track(&track).unwrap();
                drop(track);

                logger::debug("metadata loaded without hash");

                Ok(result)
            }
        }
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

    let music_path =
        "D:\\music\\Three Days Grace [restored]\\2006 - One-X\\03. Animal I Have Become.flac";

    let db = MusicDB::open(&hash_path);

    match db {
        Ok(db) => {
            let all_tracks = db.get_all_tracks().unwrap();
            let tracks_hash = all_tracks.iter().collect();
            let metadata = Metadata::from(music_path, &db, tracks_hash);

            logger::debug(&format!("{}", metadata.unwrap()));
        }
        Err(err) => logger::error(&err.to_string()),
    }
}
