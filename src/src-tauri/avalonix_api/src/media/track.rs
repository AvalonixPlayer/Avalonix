use core::fmt;
use std::{
    fs::{self, File},
    path::Path,
    time::{Duration, UNIX_EPOCH},
};

use anyhow::{Result, bail};
use lofty::{
    config::ParseOptions,
    file::{self, AudioFile, TaggedFile, TaggedFileExt},
    picture::{Picture, PictureBuilder},
    probe::Probe,
    tag::{
        Accessor,
        ItemKey::{self, Lyricist},
        ItemValue, TagExt, TagItem,
    },
};
use rcue::{cue::Cue, parser::parse_from_file};
use rkyv::{Archive, Deserialize, Serialize, rancor::Error};
use rustc_serialize::base64::{MIME, ToBase64};
use sled::Db;
use ts_rs::TS;
use uuid::Uuid;

use crate::{
    disk::{
        db::{self, DB},
        user::settings::UserSettings,
    },
    logger::{debug, error, fatal, info},
    media::{
        album::{self, Album},
        cover_get::CoverGet,
        media_array::MediaArrayType::{self},
        media_trait::Media,
        performer,
        playable_type::MediaType,
    },
};

#[derive(
    Archive, Deserialize, Serialize, Clone, serde::Serialize, serde::Deserialize, TS, Debug,
)]
#[ts(export)]
pub struct Track {
    pub uuid: String,
    pub path: String,
    #[ts(skip)]
    pub modified: Duration,
    pub source_path: String,
    // media metadata
    pub title: String,
    pub album: String,
    pub performer: String,
    pub genre: String,
    #[ts(skip)]
    pub start_time: Duration,
    #[ts(skip)]
    pub end_time: Duration,
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

        if let Ok(cue) = parse_from_file(path.as_ref(), true) {
            let create_new_tracks_with_save = || -> Result<()> {
                let tracks = Self::tracks_from_cue(cue, &path, modified)?;
                for track in tracks {
                    db.add_to_db(&track)?;
                    debug(format!("new track created: {}", track));
                    result.push(track);
                }
                Ok(())
            };
            if let Some(track) = every_tracks_in_db
                .iter()
                .find(|track| track.source_path == path.as_ref())
            {
                if modified == track.modified {
                    result.push(track.clone());
                } else {
                    db.remove_from_db(track)?;
                    create_new_tracks_with_save()?;
                }
            } else {
                create_new_tracks_with_save()?;
            }
        } else {
            if every_tracks_in_db
                .iter()
                .any(|t| t.path == path.as_ref() && t.source_path != t.path)
            {
                return Ok(vec![]);
            }

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

        let genre = tag
            .genre()
            .as_deref()
            .map_or("Unknown genre", |v| v)
            .to_string();

        let uuid = Uuid::new_v4().to_string();

        let result = Self {
            uuid,
            path: path.as_ref().to_string(),
            source_path: path.as_ref().to_string(),
            modified,
            title,
            album,
            performer,
            genre,
            start_time: Duration::new(0, 0),
            end_time: tagged_file.properties().duration(),
        };
        debug(format!("track: {} loaded without hash", result.uuid));
        Ok(result)
    }

    fn tracks_from_cue<P: AsRef<str>>(cue: Cue, path: P, modified: Duration) -> Result<Vec<Track>> {
        let mut result = vec![];

        let p = Path::new(path.as_ref()).parent().unwrap();
        let genre = if let Some(genre) = cue
            .comments
            .iter()
            .find(|comment| comment.0.starts_with("GENRE"))
        {
            let res = genre.1.to_string();
            if genre.1.to_string() == "" {
                "Unknown genre".to_string()
            } else {
                res
            }
        } else {
            "Unknown genre".to_string()
        };

        for file in cue.files {
            let tracks_len = file.tracks.len();

            for (i, track) in file.tracks.iter().enumerate() {
                let start_time = track.indices[0].1;

                let play_file_path = p.join(file.file.clone()).to_str().unwrap().to_string();

                let end_time = if i + 1 < tracks_len {
                    file.tracks[i + 1].indices[0].1
                } else {
                    let options =
                        ParseOptions::new().parsing_mode(lofty::config::ParsingMode::Relaxed);
                    let tagged_file = Probe::open(&play_file_path)?
                        .options(options)
                        .guess_file_type()?
                        .read()?;
                    tagged_file.properties().duration()
                };

                result.push(Self {
                    uuid: Uuid::new_v4().to_string(),
                    path: play_file_path,
                    modified: modified,
                    source_path: path.as_ref().to_string(),
                    title: track
                        .title
                        .clone()
                        .map_or("Unknown track".to_string(), |f| f)
                        .to_string(),
                    album: cue
                        .title
                        .clone()
                        .map_or("Unknown track".to_string(), |f| f)
                        .to_string(),
                    performer: cue
                        .performer
                        .clone()
                        .map_or("Unknown performer".to_string(), |f| f)
                        .to_string(),
                    genre: genre.clone(),
                    start_time: start_time,
                    end_time: end_time,
                });
            }
        }

        Ok(result)
    }

    pub fn edit_metadata(
        &mut self,
        db: &DB,
        title: String,
        album: String,
        performer: String,
        genre: String,
        cover_path: Option<String>,
    ) -> Result<()> {
        let old_album = self.album.clone();
        let old_performer = self.performer.clone();

        if self.path != self.source_path {
            bail!("Track from cue")
        }
        let options = ParseOptions::new().parsing_mode(lofty::config::ParsingMode::Relaxed);
        let mut tagged_file = Probe::open(&self.path)?
            .options(options)
            .guess_file_type()?
            .read()?;

        let tag = match tagged_file.primary_tag_mut() {
            Some(primary_tag) => primary_tag,
            None => {
                if let Some(first_tag) = tagged_file.first_tag_mut() {
                    first_tag
                } else {
                    let tag_type = tagged_file.primary_tag_type();
                    tagged_file.insert_tag(lofty::tag::Tag::new(tag_type));
                    tagged_file.primary_tag_mut().unwrap()
                }
            }
        };

        tag.set_title(title.clone());
        tag.set_album(album.clone());
        tag.set_artist(performer.clone());
        tag.set_genre(genre.clone());

        if let Some(path) = cover_path {
            let bytes = fs::read(path)?;
            let pic = Picture::unchecked(bytes).build();
            tag.set_picture(0, pic);
        }

        let mut file = std::fs::OpenOptions::new()
            .read(true)
            .write(true)
            .open(&self.path)?;

        tag.save_to(&mut file, lofty::config::WriteOptions::default())?;

        self.title = title;
        self.album = album;
        self.performer = performer;
        self.genre = genre;

        if self.album != old_album {}

        _ = db.update_in_db(self).map_err(|err| error(err.to_string()));

        if old_album != self.album {
            db.add_track_to_media_array(&self.album, MediaArrayType::Album, self);
        }
        if old_performer != self.performer {
            db.add_track_to_media_array(&self.performer, MediaArrayType::Performer, self);
        }

        info("Track updated");
        Ok(())
    }
}

impl fmt::Display for Track {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(
            f,
            "\ttrack: {}\n\tmodified: {}\n\tpath: {}\n\tsource: {}\n\ttitle: {}\n\talbum: {}\n\tperformer: {}",
            self.uuid,
            self.modified.as_secs(),
            self.path,
            self.source_path,
            self.title,
            self.album,
            self.performer
        )
    }
}

impl Media for Track {
    fn get_media_type(&self) -> MediaType {
        MediaType::Track
    }

    fn name_starts_with<P: AsRef<str>>(&self, start: P) -> bool {
        self.title
            .to_lowercase()
            .starts_with(&start.as_ref().to_lowercase())
    }

    fn convert_to_db(&self) -> anyhow::Result<(Vec<u8>, Vec<u8>)> {
        let value = rkyv::to_bytes::<Error>(self)?.to_vec();
        let uuid = rkyv::to_bytes::<Error>(&self.uuid)?.to_vec();

        Ok((uuid, value))
    }

    fn get_tracks_uuids(&self) -> Vec<String> {
        return vec![self.uuid.clone()];
    }

    fn get_uuid(&self) -> String {
        self.uuid.clone()
    }
}

impl CoverGet for Track {
    fn get_cover_as_uri(&self) -> String {
        let result: Result<String> = if self.source_path == self.path {
            let f = || -> Result<String> {
                let options = ParseOptions::new().parsing_mode(lofty::config::ParsingMode::Relaxed);
                let tagged_file = Probe::open(&self.path)?.options(options).read()?;

                if let Some(tag) = tagged_file.primary_tag().or(tagged_file.first_tag()) {
                    if let Some(picture) = tag.pictures().first() {
                        let data = picture.data();

                        let bs64 = data.to_base64(MIME);
                        let string = format!("data:image/png;base64,{}", bs64);
                        return Ok(string);
                    }
                }

                Ok(String::new())
            };
            f()
        } else {
            Ok(String::new())
        };
        result.unwrap_or_default()
    }
}
