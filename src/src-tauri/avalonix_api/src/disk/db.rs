use std::{
    format, fs,
    sync::{Arc, Mutex, mpsc::Sender},
    todo,
};

use anyhow::{Result, anyhow, bail};
use better_sms::mutex::MutexWork;
use glob::glob;
use rkyv::rancor::Error;

use crate::{
    disk::{disk_paths::avalonix_db, user::settings::UserSettings},
    events::Event,
    logger::{debug, error},
    media::{
        album::Album,
        media_array::{MediaArray, MediaArrayType},
        media_trait::Media,
        performer::Performer,
        playable_type::{MediaType, PlayableResult},
        playlist::Playlist,
        track::Track,
    },
};

/// Media database
pub struct DB {
    events_sender: Arc<Mutex<Sender<Event>>>,
    tracks_tree: sled::Tree,
    albums_tree: sled::Tree,
    performers_tree: sled::Tree,
    playlists_tree: sled::Tree,
}

const EXTS_TO_LIB: [&str; 4] = [".mp3", ".flac", ".wav", ".cue"];

impl DB {
    /// Opens avalonix media database
    pub fn open(event_sender: &Arc<Mutex<Sender<Event>>>) -> Result<Self> {
        let db = sled::open(avalonix_db()?)?;
        let tracks_tree = db.open_tree("tracks")?;
        let albums_tree = db.open_tree("albums")?;
        let performers_tree = db.open_tree("performers")?;
        let playlists_tree = db.open_tree("playlists")?;

        let result = Self {
            events_sender: event_sender.clone(),
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

    pub fn update_in_db<T>(&self, media: &T) -> Result<()>
    where
        T: Media,
    {
        let (_, value) = media.convert_to_db()?;

        let tree = match media.get_media_type() {
            MediaType::Track => &self.tracks_tree,
            MediaType::Album => &self.albums_tree,
            MediaType::Performer => &self.performers_tree,
            MediaType::Playlist => &self.playlists_tree,
        };
        let uuid = rkyv::to_bytes::<Error>(&media.get_uuid())?.to_vec();
        tree.insert(uuid, value)?;

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

    pub fn add_track_to_media_array<P: AsRef<str>>(
        &self,
        media_array_name: P,
        media_array_type: MediaArrayType,
        track: &Track,
    ) -> Result<()> {
        match media_array_type {
            MediaArrayType::Album => {
                let mut albums = self.get_every_album().unwrap();
                if let Some(album) = albums
                    .iter_mut()
                    .find(|album| album.title == media_array_name.as_ref())
                {
                    album.add_track(self, track.uuid.clone());
                } else {
                    Album::create_new_for_track(self, track);
                }
            }
            MediaArrayType::Performer => {
                let mut performers = self.get_every_performer().unwrap();
                if let Some(performer) = performers
                    .iter_mut()
                    .find(|performer| performer.title == media_array_name.as_ref())
                {
                    performer.add_track(self, track.uuid.clone());
                } else {
                    Performer::create_new_for_track(self, track);
                }
            }
            MediaArrayType::Playlist => {
                let mut playlists = self.get_every_performer().unwrap();
                if let Some(playlist) = playlists
                    .iter_mut()
                    .find(|playlist| playlist.title == media_array_name.as_ref())
                {
                    playlist.add_track(self, track.uuid.clone());
                } else {
                    error("Playlist don`t exists");
                }
            }
        }

        Ok(())
    }

    pub fn remove_track_from_media_array<P: AsRef<str>>(
        &self,
        media_array_name: P,
        media_array_type: MediaArrayType,
        track: &Track,
    ) -> Result<()> {
        match media_array_type {
            MediaArrayType::Album => {
                let mut albums = self.get_every_album().unwrap();
                if let Some(album) = albums
                    .iter_mut()
                    .find(|album| album.title == media_array_name.as_ref())
                {
                    album.remove_track(self, track.uuid.clone());
                } else {
                    Album::create_new_for_track(self, track);
                }
            }
            MediaArrayType::Performer => {
                let mut performers = self.get_every_performer().unwrap();
                if let Some(performer) = performers
                    .iter_mut()
                    .find(|performer| performer.title == media_array_name.as_ref())
                {
                    performer.remove_track(self, track.uuid.clone());
                } else {
                    Performer::create_new_for_track(self, track);
                }
            }
            MediaArrayType::Playlist => {
                let mut playlists = self.get_every_performer().unwrap();
                if let Some(playlist) = playlists
                    .iter_mut()
                    .find(|playlist| playlist.title == media_array_name.as_ref())
                {
                    playlist.add_track(self, track.uuid.clone());
                } else {
                    error("Playlist don`t exists");
                }
            }
        }

        Ok(())
    }

    pub fn get_uuids(&self, media_type: MediaType) -> Result<Vec<String>> {
        let mut result = vec![];
        let media_tree = match media_type {
            MediaType::Track => &self.tracks_tree,
            MediaType::Album => &self.albums_tree,
            MediaType::Performer => &self.performers_tree,
            MediaType::Playlist => &self.playlists_tree,
        };

        for media in media_tree {
            let (id, _val) = media?;
            let id: String = rkyv::from_bytes::<String, Error>(&id)?;
            result.push(id);
        }

        Ok(result)
    }

    pub fn get_media_by_id(&self, id: String, media_type: MediaType) -> Result<PlayableResult> {
        match media_type {
            MediaType::Track => self
                .get_every_track()?
                .into_iter()
                .find(|t| t.uuid == id)
                .map(PlayableResult::Track)
                .ok_or_else(|| anyhow!("Track with id {} not found", id)),

            MediaType::Album => self
                .get_every_album()?
                .into_iter()
                .find(|a| a.uuid == id)
                .map(PlayableResult::Album)
                .ok_or_else(|| anyhow!("Album with id {} not found", id)),

            MediaType::Performer => self
                .get_every_performer()?
                .into_iter()
                .find(|p| p.uuid == id)
                .map(PlayableResult::Performer)
                .ok_or_else(|| anyhow!("Performer with id {} not found", id)),

            MediaType::Playlist => self
                .get_every_playlist()?
                .into_iter()
                .find(|p| p.uuid == id)
                .map(PlayableResult::Playlist)
                .ok_or_else(|| anyhow!("Playlist with id {} not found", id)),
        }
    }

    pub fn get_ids_by_part_of_name(
        &self,
        part_of_name: &String,
        media_type: MediaType,
    ) -> Result<Vec<String>> {
        let mut result = vec![];
        match media_type {
            MediaType::Track => {
                for media in &self.tracks_tree {
                    let (id, value) = media?;
                    let item: Track = rkyv::from_bytes::<Track, Error>(&value)?;
                    let id: String = rkyv::from_bytes::<String, Error>(&id)?;
                    if item.name_starts_with(part_of_name) {
                        result.push(id);
                    }
                }
            }
            MediaType::Album => {
                for media in &self.albums_tree {
                    let (id, value) = media?;
                    let item: Album = rkyv::from_bytes::<Album, Error>(&value)?;
                    let id: String = rkyv::from_bytes::<String, Error>(&id)?;
                    if item.name_starts_with(part_of_name) {
                        result.push(id);
                    }
                }
            }
            MediaType::Performer => {
                for media in &self.performers_tree {
                    let (id, value) = media?;
                    let item: Performer = rkyv::from_bytes::<Performer, Error>(&value)?;
                    let id: String = rkyv::from_bytes::<String, Error>(&id)?;
                    if item.name_starts_with(part_of_name) {
                        result.push(id);
                    }
                }
            }
            MediaType::Playlist => {
                for media in &self.playlists_tree {
                    let (id, value) = media?;
                    let item: Playlist = rkyv::from_bytes::<Playlist, Error>(&value)?;
                    let id: String = rkyv::from_bytes::<String, Error>(&id)?;
                    if item.name_starts_with(part_of_name) {
                        result.push(id);
                    }
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

    pub fn update(&self, settings: &mut UserSettings) -> Result<()> {
        let tracks = &self.get_every_track()?[0..];

        for folder in &settings.library_paths {
            let folder = glob::Pattern::escape(folder);
            for ext in EXTS_TO_LIB {
                for entry in
                    glob(&format!("{}/**/*{}", folder, ext).to_string()).expect("can`t to read")
                {
                    match entry {
                        Ok(path) => {
                            if let Ok(_) =
                                Track::get_tracks_by_path(path.to_str().unwrap(), tracks, self)
                            {
                            }
                        }
                        Err(err) => {
                            error(err.to_string());
                        }
                    }
                }
            }
        }

        for track in self.get_every_track()? {
            if !fs::exists(&track.source_path)? {
                self.remove_from_db(&track)?;
                debug(format!("not exists path removed: {}", track.path));
            }

            if settings.path_removed == true {
                let mut contains = false;
                for folder in &settings.library_paths {
                    if track.source_path.contains(folder) {
                        contains = true;
                    }
                }

                if contains == false {
                    debug(format!(
                        "not exists in lib paths track removed: {}",
                        track.path
                    ));
                    self.remove_from_db(&track)?;
                }
            }
        }

        if settings.path_removed == true {
            settings.path_removed = false;
        }

        let actual_tracks = self.get_every_track()?;

        Album::create_albums(self, &actual_tracks)?;
        Performer::create_performers(self, &actual_tracks)?;
        self.events_sender
            .lock_unw()
            .send(Event::UpdateLibrary)
            .unwrap();
        Ok(())
    }

    pub fn edit_track(
        &self,
        uuid: String,
        title: String,
        album: String,
        performer: String,
        genre: String,
        cover_path: Option<String>,
    ) -> Result<()> {
        let media = self.get_media_by_id(uuid, MediaType::Track)?;
        media
            .unwrap_as_track()
            .edit_metadata(self, title, album, performer, genre, cover_path)?;
        Ok(())
    }

    pub fn edit_album(&self, uuid: String, title: String, performer: String) -> Result<()> {
        let media = self.get_media_by_id(uuid, MediaType::Album)?;
        media
            .unwrap_as_album()
            .edit_metadata(self, title, performer)?;
        Ok(())
    }

    pub fn add_track_to_playlist(&self, playlist_uuid: String, track_uuid: String) -> Result<()> {
        if let Some(playlist) = self
            .get_every_playlist()?
            .iter_mut()
            .find(|playlist| playlist.uuid == playlist_uuid)
        {
            if let Some(_) = playlist
                .tracks_ids
                .iter()
                .find(|track| **track == track_uuid)
            {
                debug("track now in playlist btw");
            } else {
                playlist.tracks_ids.push(track_uuid);
                self.add_to_db(playlist)?;
            }
        }
        Ok(())
    }

    pub fn remove_playlist(&self, playlist_uuid: String) -> Result<()> {
        if let Some(playlist) = self
            .get_every_playlist()?
            .iter_mut()
            .find(|playlist| playlist.uuid == playlist_uuid)
        {
            self.remove_from_db(playlist)?;
        }
        Ok(())
    }
}
