use std::fs;

use anyhow::Result;
use glob::glob;
use rkyv::rancor::Error;

use crate::{
    disk::{disk_paths::avalonix_db, user::settings::UserSettings},
    logger::{debug, error, fatal},
    media::{
        album::Album, media_trait::Media, performer::Performer, playable_type::MediaType,
        track::Track,
    },
};

/// Media database
pub struct DB {
    tracks_tree: sled::Tree,
    albums_tree: sled::Tree,
    performers_tree: sled::Tree,
}

const EXTS_TO_LIB: [&str; 4] = [".mp3", ".flac", ".wav", ".cue"];

impl DB {
    /// Opens avalonix media database
    pub fn open() -> Result<Self> {
        let db = sled::open(avalonix_db()?)?;
        let tracks_tree = db.open_tree("tracks")?;
        let albums_tree = db.open_tree("albums")?;
        let performers_tree = db.open_tree("performers")?;

        let result = Self {
            tracks_tree,
            albums_tree,
            performers_tree,
        };
        Ok(result)
    }

    /// Adds media to db
    pub fn add_to_db<T>(&self, media: &T) -> Result<()>
    where
        T: Media,
    {
        let (key, value) = media.convert_to_db()?;
        let tree = match media.get_media_type() {
            MediaType::Track => &self.tracks_tree,
            MediaType::Album => &self.albums_tree,
            MediaType::Performer => &self.performers_tree,
        };
        tree.insert(key, value)?;

        Ok(())
    }

    /// Removes media from db
    pub fn remove_from_db<T>(&self, media: &T) -> Result<()>
    where
        T: Media,
    {
        let (key, _) = media.convert_to_db()?;
        let tree = match media.get_media_type() {
            MediaType::Track => &self.tracks_tree,
            MediaType::Album => &self.albums_tree,
            MediaType::Performer => &self.performers_tree,
        };
        tree.remove(key)?;
        Ok(())
    }

    pub fn get_uuids(&self, media_type: MediaType) -> Result<Vec<String>> {
        let mut result = vec![];
        match media_type {
            MediaType::Track => {
                for media in &self.tracks_tree {
                    let (id, val) = media?;
                    let id: String = rkyv::from_bytes::<String, Error>(&id)?;
                    result.push(id);
                }
            }
            MediaType::Album => {
                for media in &self.albums_tree {
                    let (id, _) = media?;
                    let id: String = rkyv::from_bytes::<String, Error>(&id)?;
                    result.push(id);
                }
            }
            MediaType::Performer => {
                for media in &self.performers_tree {
                    let (id, _) = media?;
                    let id: String = rkyv::from_bytes::<String, Error>(&id)?;
                    result.push(id);
                }
            }
        }
        Ok(result)
    }

    /// Gets all tracks from the database.
    pub fn get_every_track(&self) -> Result<Vec<Track>> {
        let mut tracks = Vec::new();
        for media in &self.tracks_tree {
            let (_, value) = media?;
            let item: Track = rkyv::from_bytes::<Track, Error>(&value)?;
            tracks.push(item);
        }
        Ok(tracks)
    }

    /// Gets all albums from the database.
    pub fn get_every_album(&self) -> Result<Vec<Album>> {
        let mut albums = Vec::new();
        for media in &self.albums_tree {
            let (_, value) = media?;
            let item: Album = rkyv::from_bytes::<Album, Error>(&value)?;
            albums.push(item);
        }
        Ok(albums)
    }

    /// Gets all performers from the database.
    pub fn get_every_performer(&self) -> Result<Vec<Performer>> {
        let mut performers = Vec::new();
        for media in &self.performers_tree {
            let (_, value) = media?;
            let item: Performer = rkyv::from_bytes::<Performer, Error>(&value)?;
            performers.push(item);
        }
        Ok(performers)
    }

    pub fn update(&self, settings: &UserSettings) -> Result<()> {
        let tracks = &self.get_every_track()?[0..];
        let mut tracks_to_append = vec![];

        for folder in &settings.library_paths {
            let folder = glob::Pattern::escape(folder);
            for ext in EXTS_TO_LIB {
                for entry in
                    glob(&format!("{}/**/*{}", folder, ext).to_string()).expect("can`t to read")
                {
                    match entry {
                        Ok(path) => {
                            tracks_to_append.push(Track::get_tracks_by_path(
                                path.to_str().unwrap(),
                                tracks,
                                self,
                            ));
                        }
                        Err(err) => {
                            error(err.to_string());
                        }
                    }
                }
            }
        }

        for track in self.get_every_track()? {
            if !fs::exists(&track.path)? {
                self.remove_from_db(&track)?;
                debug(format!("not exists path removed: {}", track.path));
            }
        }

        Album::create_albums(self, tracks)?;
        Performer::create_performers(self, tracks)?;
        Ok(())
    }
}
