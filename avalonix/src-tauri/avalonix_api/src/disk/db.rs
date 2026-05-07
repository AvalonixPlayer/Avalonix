use std::{
    fs,
    sync::{Arc, Mutex},
    time::UNIX_EPOCH,
};

use anyhow::Ok;
use rkyv::rancor::Error;

use crate::{
    disk::{db, disk_manager, settings::Settings},
    logger,
    media::{album::Album, performer::Performer, track::Track, tracks_group::TracksGroup},
    metadata::{
        album_filter_metadata::AlbumFilterMetadata, filter_metadata::FilterMetadata,
        performer_filter_metadata::PerformerFilterMetadata,
        track_filter_metadata::TrackFilterMetadata,
    },
    mutex_work::CreateArcMutex,
};

/// Local library database storing hash
pub struct DB {
    /// tracks tree
    tracks: sled::Tree,
    /// albums tree
    albums: sled::Tree,
    /// performers tree
    performers: sled::Tree,
}

impl DB {
    /// Creates or opens a db
    pub fn open() -> anyhow::Result<Arc<Mutex<Self>>> {
        let db_path = disk_manager::db_path();
        let db = sled::open(db_path)?;

        let tracks_tree = db.open_tree(b"tracks")?;
        let albums_tree = db.open_tree(b"albums")?;
        let performers_tree = db.open_tree(b"performers")?;

        let db = Self {
            tracks: tracks_tree,
            albums: albums_tree,
            performers: performers_tree,
        };

        Ok(db.create_arc_mutex())
    }

    /// Searches for files in the directories specified in the settings and adds or updates data to the library based on them
    pub fn update_library(&mut self, settings: &Settings) -> anyhow::Result<()> {
        self.update_tracks_library(settings)?;
        self.update_albums_library()?;
        self.update_performers_library()?;

        Ok(())
    }

    fn restart_db(&mut self) -> anyhow::Result<()> {
        let db_path = disk_manager::db_path();
        let db = sled::open(db_path)?;

        let tracks_tree = db.open_tree(b"tracks")?;
        let albums_tree = db.open_tree(b"albums")?;
        let performers_tree = db.open_tree(b"performers")?;
        drop(db);

        self.tracks = tracks_tree;
        self.albums = albums_tree;
        self.performers = performers_tree;
        Ok(())
    }

    /// Cleans the library and removes it from the disk, restarting the db to avoid errors
    pub fn clear_library(&mut self, settings: &mut Settings) -> anyhow::Result<()> {
        self.clear_tracks()?;
        self.clear_albums()?;
        self.clear_performers()?;
        let db_path = disk_manager::db_path();
        fs::remove_dir_all(&db_path)?;
        settings.lib_paths.clear();
        settings.save()?;
        logger::debug("library cleared");
        self.restart_db()?;

        Ok(())
    }
}

impl DB {
    /// Saves the track to the database
    pub fn save_track(&self, track: Track) -> anyhow::Result<()> {
        let key = &track.metadata.id;

        let value_bytes = rkyv::to_bytes::<Error>(&track)?.to_vec();

        self.tracks.insert(key, value_bytes)?;
        self.tracks.flush()?;

        logger::info(format!("track saved to db: {}", track));
        Ok(())
    }

    /// Returns the primary metadata of each track in the library
    pub fn get_tracks_filter_metadatas(&self) -> anyhow::Result<Vec<TrackFilterMetadata>> {
        let mut result = vec![];
        for track in &self.tracks {
            let (_, track) = track?;
            let track = rkyv::from_bytes::<Track, Error>(&track)?;
            result.push(track.metadata.get_filter_metadata()?);
        }
        Ok(result)
    }

    /// Returns the completely metadata of each track in the library
    pub fn get_tracks_hash(&self) -> anyhow::Result<Vec<Track>> {
        let mut result = vec![];
        for track in &self.tracks {
            let (_, track) = track?;
            let track = rkyv::from_bytes::<Track, Error>(&track)?;
            result.push(track);
        }
        Ok(result)
    }

    fn clear_tracks(&self) -> anyhow::Result<()> {
        self.tracks.clear()?;
        self.tracks.flush()?;
        Ok(())
    }

    fn update_tracks_library(&self, settings: &Settings) -> anyhow::Result<()> {
        let mut tracks = vec![];
        let tracks_files_paths = disk_manager::get_tracks_files_paths(settings);
        let tracks_hash = self.get_tracks_hash()?;
        logger::debug("Update tracks lib");

        for track_file_path in tracks_files_paths {
            if let Some(track_in_lib) = tracks_hash
                .iter()
                .find(|x| *x.start_file_path == track_file_path.to_str().unwrap().to_string())
            {
                let mod_time = fs::metadata(&track_file_path)?
                    .modified()?
                    .duration_since(UNIX_EPOCH)?
                    .as_secs();
                if mod_time != track_in_lib.mod_date {
                    logger::debug("track metadata must to reload");
                    let tracks_b = Track::create_tracks_list_from_file(track_file_path, &self);
                    match tracks_b {
                        Result::Ok(mut tracks_b) => {
                            tracks.append(&mut tracks_b);
                        }
                        Err(err) => logger::warn(err),
                    }
                }
            } else {
                let tracks_b = Track::create_tracks_list_from_file(track_file_path, &self);
                match tracks_b {
                    Result::Ok(mut tracks_b) => {
                        tracks.append(&mut tracks_b);
                    }
                    Err(err) => logger::warn(err),
                }
            }
        }

        for track in tracks {
            self.save_track(track)?;
        }
        Ok(())
    }
}

impl DB {
    /// Saves the album to the database
    pub fn save_album(&self, album: Album) -> anyhow::Result<()> {
        let key = &album.album_metadata.id;

        let value_bytes = rkyv::to_bytes::<Error>(&album)?.to_vec();

        self.albums.insert(key, value_bytes)?;
        self.albums.flush()?;

        logger::info(format!("album saved to db: {}", album));

        Ok(())
    }

    /// Returns the IDs of all albums
    pub fn get_albums_ids(&self) -> anyhow::Result<Vec<Vec<u8>>> {
        let mut result = vec![];
        for album in &self.albums {
            let (id, _) = album?;
            result.push(id.to_vec());
        }
        Ok(result)
    }

    /// Returns the completely metadata of each album in the library
    pub fn get_albums_hash(&self) -> anyhow::Result<Vec<Album>> {
        let mut result = vec![];
        for album in &self.albums {
            let (_, album) = album?;
            let album = rkyv::from_bytes::<Album, Error>(&album)?;
            result.push(album);
        }
        Ok(result)
    }

    fn clear_albums(&self) -> anyhow::Result<()> {
        self.albums.clear()?;
        self.albums.flush()?;
        Ok(())
    }

    fn update_albums_library(&mut self) -> anyhow::Result<()> {
        logger::debug("Update albums lib");

        let tracks_hash = &self.get_tracks_hash()?;
        let albums_hash = &self.get_albums_hash()?;
        let new_albums = Album::group_tracks(albums_hash, tracks_hash)?;

        for album in new_albums {
            if let Some(exists_album) = albums_hash
                .iter()
                .find(|a| a.tracks_ids == album.tracks_ids)
            {
                logger::debug(format!(
                    "Album with name: {} dont needs to save",
                    exists_album.album_metadata.album_title
                ));
                continue;
            }
            self.save_album(album)?;
        }

        Ok(())
    }

    /// Returns the primary metadata of each album in the library
    pub fn get_albums_filter_datas(&mut self) -> anyhow::Result<Vec<AlbumFilterMetadata>> {
        let mut result = vec![];
        for album in &self.albums {
            let (_, album) = album?;
            let album = rkyv::from_bytes::<Album, Error>(&album)?;
            result.push(album.album_metadata.get_filter_metadata()?);
        }
        Ok(result)
    }
}

impl DB {
    /// Saves the performer to the database
    pub fn save_performer(&self, performer: Performer) -> anyhow::Result<()> {
        let key = &performer.performer_metadata.id;

        let value_bytes = rkyv::to_bytes::<Error>(&performer)?.to_vec();

        self.performers.insert(key, value_bytes)?;
        self.performers.flush()?;

        logger::info(format!("performer saved to db: {}", performer));

        Ok(())
    }

    /// Returns the IDs of all performers
    pub fn get_performers_ids(&self) -> anyhow::Result<Vec<Vec<u8>>> {
        let mut result = vec![];
        for performer in &self.performers {
            let (id, _) = performer?;
            result.push(id.to_vec());
        }
        Ok(result)
    }

    /// Returns the completely metadata of each performer in the library
    pub fn get_performers_hash(&self) -> anyhow::Result<Vec<Performer>> {
        let mut result = vec![];
        for performer in &self.performers {
            let (_, performer) = performer?;
            let performer = rkyv::from_bytes::<Performer, Error>(&performer)?;
            result.push(performer);
        }
        Ok(result)
    }

    fn clear_performers(&self) -> anyhow::Result<()> {
        self.performers.clear()?;
        self.performers.flush()?;
        Ok(())
    }

    fn update_performers_library(&mut self) -> anyhow::Result<()> {
        logger::debug("Update albums lib");

        let tracks_hash = &self.get_tracks_hash()?;
        let performers_hash = &self.get_performers_hash()?;
        let new_performers = Performer::group_tracks(performers_hash, tracks_hash)?;

        for performer in new_performers {
            if let Some(exists_performer) = performers_hash
                .iter()
                .find(|a| a.tracks_ids == performer.tracks_ids)
            {
                logger::debug(format!(
                    "Album with name: {} dont needs to save",
                    exists_performer.performer_metadata.performer_title
                ));
                continue;
            }
            self.save_performer(performer)?;
        }

        Ok(())
    }

    /// Returns the primary metadata of each performer in the library
    pub fn get_performers_filter_datas(&mut self) -> anyhow::Result<Vec<PerformerFilterMetadata>> {
        let mut result = vec![];
        for album in &self.performers {
            let (_, album) = album?;
            let album = rkyv::from_bytes::<Performer, Error>(&album)?;
            result.push(album.performer_metadata.get_filter_metadata()?);
        }
        Ok(result)
    }
}
