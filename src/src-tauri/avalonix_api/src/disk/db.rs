use std::fs;

use anyhow::Result;
use rkyv::rancor::Error;

use crate::{
    disk::disk_paths::avalonix_db,
    logger::debug,
    media::{
        album::Album, media_trait::Media, performer::Performer, playable_type::MediaType,
        playlist::Playlist, track::Track,
    },
};

/// Media database
pub struct DB {
    tracks_tree: sled::Tree,
    albums_tree: sled::Tree,
    performers_tree: sled::Tree,
    playlists_tree: sled::Tree,
}

impl DB {
    /// Opens avalonix media database
    pub fn open() -> Result<Self> {
        let db = sled::open(avalonix_db()?)?;
        let tracks_tree = db.open_tree("tracks")?;
        let albums_tree = db.open_tree("albums")?;
        let performers_tree = db.open_tree("performers")?;
        let playlists_tree = db.open_tree("playlists")?;

        let result = Self {
            tracks_tree,
            albums_tree,
            performers_tree,
            playlists_tree,
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
            MediaType::Playlist => &self.playlists_tree,
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
            MediaType::Playlist => &self.playlists_tree,
        };
        tree.remove(key)?;
        Ok(())
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

    /// Gets all playlists from the database.
    pub fn get_every_playlist(&self) -> Result<Vec<Playlist>> {
        let mut playlists = Vec::new();
        for media in &self.playlists_tree {
            let (_, value) = media?;
            let item: Playlist = rkyv::from_bytes::<Playlist, Error>(&value)?;
            playlists.push(item);
        }
        Ok(playlists)
    }

    pub fn update(&self) -> Result<()> {
        for track in self.get_every_track()? {
            if !fs::exists(&track.path)? {
                self.remove_from_db(&track)?;
                debug(format!("not exists path removed: {}", track.path));
            }
        }
        let tracks = &self.get_every_track()?[0..];
        Album::create_albums(self, tracks)?;
        Performer::create_performers(self, tracks)?;
        Ok(())
    }
}
