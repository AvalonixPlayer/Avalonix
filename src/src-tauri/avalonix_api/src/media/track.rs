use core::fmt;
use std::{
    fs,
    time::{Duration, UNIX_EPOCH},
};

use anyhow::Result;
use lofty::{config::ParseOptions, file::TaggedFileExt, probe::Probe, tag::Accessor};
use rkyv::{Archive, Deserialize, Serialize, rancor::Error};
use uuid::Uuid;

use crate::{disk::db::DB, logger::debug, media::media_trait::Media};

#[derive(Archive, Deserialize, Serialize, Clone)]
pub struct Track {
    pub uuid: String,
    pub path: String,
    pub modified: Duration,
    // media metadata
    pub title: String,
    pub album: String,
    pub performer: String,
}

impl Track {
    /// Automatically creates an array of tracks by path
    pub fn get_tracks_by_path<P: AsRef<str>>(
        path: P,
        every_tracks_in_db: &[Track],
        db: &DB,
    ) -> Result<Vec<Track>> {
        let fs_metadata = fs::metadata(path.as_ref())?;
        let mut result = vec![];

        let modified = fs_metadata.modified()?.duration_since(UNIX_EPOCH)?;

        let mut create_new_track_with_save = || -> Result<()> {
            let track = Self::new_track(&path, modified)?;
            db.add_to_db(&track)?;
            result.push(track);
            Ok(())
        };

        if let Some(track) = every_tracks_in_db
            .iter()
            .find(|track| track.path == path.as_ref())
        {
            if modified == track.modified {
                result.push(track.clone());
            } else {
                db.remove_from_db(track)?;
                create_new_track_with_save()?;
            }
        } else {
            create_new_track_with_save()?;
        }

        Ok(result)
    }

    fn new_track<P: AsRef<str>>(path: P, modified: Duration) -> Result<Track> {
        let options = ParseOptions::new().parsing_mode(lofty::config::ParsingMode::Relaxed);
        let tagged_file = Probe::open(&path.as_ref())?
            .options(options)
            .guess_file_type()?
            .read()?;

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
        let performer = tag
            .artist()
            .as_deref()
            .map_or("Unknown artist", |v| v)
            .to_string();
        let uuid = Uuid::new_v4().to_string();
        let result = Self {
            uuid,
            path: path.as_ref().to_string(),
            modified,
            title,
            album,
            performer,
        };
        debug(format!("track: {} loaded without hash", result.uuid));
        Ok(result)
    }
}

impl fmt::Display for Track {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(
            f,
            "\ttrack: {}\n\tmodified: {}\n\tpath: {}\n\ttitle: {}\n\talbum: {}\n\tperformer: {}",
            self.uuid,
            self.modified.as_secs(),
            self.path,
            self.title,
            self.album,
            self.performer
        )
    }
}

impl Media for Track {
    fn get_media_type(&self) -> super::media_trait::MediaType {
        super::media_trait::MediaType::Track
    }
    fn convert_to_db(&self) -> anyhow::Result<(String, Vec<u8>)> {
        let value = rkyv::to_bytes::<Error>(self)?.to_vec();
        Ok((self.uuid.clone(), value))
    }
}
