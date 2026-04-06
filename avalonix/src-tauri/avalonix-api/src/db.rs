use std::time::Instant;

use crate::{playable::album::Album, playable::track::Track, utils::get_argument_val};
use sled::{Error as SledError, IVec, Tree};

pub struct MusicDB {
    pub db: sled::Db,
    pub tracks: Tree,
    pub artists: Tree,
    pub albums: Tree,
    pub playlists: Tree,
}

pub const DEFAULT_DB_PATH: &str = ".avalonix/db";

fn to_sled_error<E: std::fmt::Display + std::error::Error + Send + Sync + 'static>(
    e: E,
) -> SledError {
    SledError::Io(std::io::Error::new(std::io::ErrorKind::InvalidData, e))
}

impl MusicDB {
    pub fn open(path: &str) -> sled::Result<Self> {
        let db = sled::open(path)?;
        Ok(MusicDB {
            tracks: db.open_tree("tracks")?,
            artists: db.open_tree("artists")?,
            albums: db.open_tree("albums")?,
            playlists: db.open_tree("playlists")?,
            db,
        })
    }

    pub fn save_track(&self, track: &Track) -> sled::Result<()> {
        let key = track.id.as_bytes();

        let bytes = bincode::serialize(track).map_err(to_sled_error)?;

        self.tracks.insert(key, bytes)?;

        self.tracks.flush()?;
        Ok(())
    }

    pub fn get_track(&self, id: &str) -> sled::Result<Option<Track>> {
        match self.tracks.get(id.as_bytes())? {
            Some(value) => {
                let track: Track = bincode::deserialize(&value).map_err(to_sled_error)?;
                Ok(Some(track))
            }
            None => Ok(None),
        }
    }

    pub fn get_all_tracks(&self) -> sled::Result<Vec<Track>> {
        let mut tracks = Vec::new();
        for item in self.tracks.iter() {
            let (_, value) = item?;
            let track: Track = bincode::deserialize(&value).map_err(to_sled_error)?;
            tracks.push(track);
        }
        Ok(tracks)
    }

    pub fn get_all_tracks_id(&self) -> sled::Result<Vec<Vec<u8>>> {
        let mut result = Vec::new();
        for i in self.tracks.iter().keys() {
            let id = i?.to_vec();
            result.push(id);
        }
        Ok(result)
    }

    pub fn get_track_by_id(&self, id: &Vec<u8>) -> sled::Result<Option<Track>> {
        let some_bytes = self.tracks.get(id)?;

        if let Some(bytes) = some_bytes {
            let track: Track = bincode::deserialize(&bytes).map_err(to_sled_error)?;
            return Ok(Some(track));
        }
        Ok(None)
    }

    pub fn get_tracks_filter_data(&self, id: &Vec<u8>) -> sled::Result<Option<Track>> {
        let some_bytes = self.tracks.get(id)?;

        if let Some(bytes) = some_bytes {
            let track: Track = bincode::deserialize(&bytes).map_err(to_sled_error)?;
            return Ok(Some(track));
        }
        Ok(None)
    }

    pub fn delete_track(&self, id: &str) -> sled::Result<()> {
        self.tracks.remove(id.as_bytes())?;
        Ok(())
    }

    pub fn save_album(&self, album: &Album) -> sled::Result<()> {
        let key = album.id.as_bytes();

        let bytes = bincode::serialize(album).unwrap();

        self.albums.insert(key, bytes)?;

        self.albums.flush()?;
        Ok(())
    }

    pub fn get_all_albums(&self) -> sled::Result<Vec<Album>> {
        let mut albums = Vec::new();
        for item in self.albums.iter() {
            let (_, value) = item?;
            let album: Album = bincode::deserialize(&value).map_err(to_sled_error)?;
            albums.push(album);
        }
        Ok(albums)
    }

    pub fn get_all_albums_id(&self) -> sled::Result<Vec<Vec<u8>>> {
        let mut result = Vec::new();

        for i in self.albums.iter().keys() {
            let id = i?.to_vec();
            result.push(id);
        }
        Ok(result)
    }

    pub fn get_album_by_id(&self, id: &Vec<u8>) -> sled::Result<Option<Album>> {
        let some_bytes = self.albums.get(id).map_err(to_sled_error)?;

        if let Some(bytes) = some_bytes {
            let album: Album = bincode::deserialize(&bytes).map_err(to_sled_error)?;
            return Ok(Some(album));
        }
        Ok(None)
    }

    pub fn get_all_album_filter_data(&self) -> sled::Result<Vec<(String, String)>> {
        let mut result = Vec::new();

        for i in self.albums.iter() {
            let (_, value) = i.unwrap();
            let album: Album = bincode::deserialize(&value).map_err(to_sled_error)?;
            let metadata = album.metadata.clone().unwrap();

            result.push((metadata.name.clone(), metadata.artist.clone()));
        }
        Ok(result)
    }

    pub fn get_size_on_disk(&self) -> u64 {
        self.db.size_on_disk().unwrap_or(0)
    }
}

#[test]
fn test_db_get_all_tracks_id() {
    use crate::disk_manager;
    use crate::logger;

    let hash_path = disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(&hash_path);

    match db {
        Ok(db) => {
            let start = Instant::now();
            match db.get_all_tracks_id() {
                Ok(_) => {}
                Err(_) => {}
            }
            logger::debug(&format!(
                "all tracks ids get: {} ms",
                start.elapsed().as_millis()
            ));
        }
        Err(err) => logger::error(&err.to_string()),
    }
}

#[test]
fn test_db_get_track_by_id() {
    use crate::disk_manager;
    use crate::logger;

    let hash_path = disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(&hash_path);

    match db {
        Ok(db) => {
            let ids = db.get_all_tracks_id().unwrap();
            let start = Instant::now();
            match db.get_track_by_id(&ids[0]) {
                Ok(track_opt) => {
                    let track = track_opt.unwrap();
                    logger::debug(&format!(
                        "track get by id: {}",
                        track.metadata.title.unwrap().clone()
                    ));
                }
                Err(_) => {}
            }
            logger::debug(&format!(
                "get track by id: {} ms",
                start.elapsed().as_millis()
            ));
        }
        Err(err) => logger::error(&err.to_string()),
    }
}

#[test]
fn test_db_get_all_albums_id() {
    use crate::disk_manager;
    use crate::logger;

    let hash_path = disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(&hash_path);

    match db {
        Ok(db) => {
            let start = Instant::now();
            match db.get_all_albums_id() {
                Ok(ids) => {
                    let len = ids.len();
                    println!("{:?}", ids[len - 1]);
                }
                Err(err) => logger::error(&err.to_string()),
            }
            logger::debug(&format!(
                "all albums ids get: {} ms",
                start.elapsed().as_millis()
            ));
        }
        Err(err) => logger::error(&err.to_string()),
    }
}

#[test]
fn test_db_get_album_by_id() {
    use crate::disk_manager;
    use crate::logger;

    let hash_path = disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(&hash_path);

    match db {
        Ok(db) => {
            let ids = db.get_all_albums_id().unwrap();

            let start = Instant::now();
            match db.get_album_by_id(&ids[0]) {
                Ok(album) => match album {
                    Some(album) => {
                        logger::debug(&format!("{}", album.metadata.unwrap().name));
                    }
                    None => {}
                },
                Err(err) => {
                    logger::error(&err.to_string());
                }
            }
            logger::debug(&format!(
                "get album by id: {} ms",
                start.elapsed().as_millis()
            ));
        }
        Err(err) => logger::error(&err.to_string()),
    }
}

#[test]
fn get_all_albums_filter_data() {
    use crate::disk_manager;
    use crate::logger;

    let hash_path = disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(&hash_path);

    let db = db.unwrap();
    for i in db.get_all_album_filter_data().unwrap() {
        logger::debug(&format!("Album: {} Artist: {}", i.0, i.1));
    }
}
