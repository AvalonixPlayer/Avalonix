use std::fs;

use anyhow::Ok;
use rkyv::rancor::Error;

use crate::{
    disk::{
        disk_manager,
        settings::{self, Settings},
    },
    logger,
    media::{
        album::{self, Album},
        track::Track,
        tracks_group::TracksGroup,
    },
    metadata::{filter_metadata::FilterMetadata, track_filter_metadata::TrackFilterMetadata},
};

pub struct DB {
    pub db_hash: DBHash,
    tracks: sled::Tree,
    albums: sled::Tree,
}

pub struct DBHash {
    pub tracks_hash: Vec<Track>,
    pub albums_hash: Vec<Album>,
}

impl DBHash {
    pub fn new() -> Self {
        Self {
            tracks_hash: vec![],
            albums_hash: vec![],
        }
    }
}

impl DB {
    pub fn open() -> anyhow::Result<Self> {
        let db_path = disk_manager::db_path();
        let db = sled::open(db_path)?;

        let tracks_tree = db.open_tree(b"tracks")?;
        let albums_tree = db.open_tree(b"albums")?;

        let db = Self {
            db_hash: DBHash::new(),
            tracks: tracks_tree,
            albums: albums_tree,
        };

        Ok(db)
    }

    pub fn update_library(&mut self, settings: &Settings) -> anyhow::Result<()> {
        self.update_tracks_library(settings)?;

        self.load_tracks_hash()?;
        self.load_albums_hash()?;
        Ok(())
    }
}

impl DB {
    pub fn save_track(&self, track: Track) -> anyhow::Result<()> {
        let key = &track.metadata.id;

        let value_bytes = rkyv::to_bytes::<Error>(&track)?.to_vec();

        self.tracks.insert(key, value_bytes)?;
        self.tracks.flush()?;

        logger::info(format!("track saved to db: {}", track));
        Ok(())
    }

    pub fn get_tracks_filter_metadatas(&self) -> anyhow::Result<Vec<TrackFilterMetadata>> {
        let mut result = vec![];
        for track in &self.tracks {
            let (_, track) = track?;
            let track = rkyv::from_bytes::<Track, Error>(&track)?;
            result.push(track.metadata.get_filter_metadata()?);
        }
        Ok(result)
    }

    pub fn load_tracks_hash(&mut self) -> anyhow::Result<()> {
        let mut result = vec![];
        for track in &self.tracks {
            let (_, track) = track?;
            let track = rkyv::from_bytes::<Track, Error>(&track)?;
            result.push(track);
        }
        self.db_hash.tracks_hash = result;
        Ok(())
    }

    pub fn clear_tracks(&self) -> anyhow::Result<()> {
        self.tracks.clear()?;
        self.tracks.flush()?;
        Ok(())
    }

    pub fn update_tracks_library(&self, settings: &Settings) -> anyhow::Result<()> {
        let mut tracks = vec![];
        let tracks_files_paths = disk_manager::get_tracks_files_paths(settings);
        for track_file_path in tracks_files_paths {
            let mut tracks_b = Track::create_tracks_list_from_file(track_file_path, &self)?;
            tracks.append(&mut tracks_b);
        }

        for track in tracks {
            self.save_track(track)?;
        }
        Ok(())
    }
}

impl DB {
    pub fn save_album(&self, album: Album) -> anyhow::Result<()> {
        let key = &album.album_metadata.id;

        let value_bytes = rkyv::to_bytes::<Error>(&album)?.to_vec();

        self.albums.insert(key, value_bytes)?;
        self.albums.flush()?;

        logger::info(format!("album saved to db: {}", album));

        Ok(())
    }

    pub fn get_albums_ids(&self) -> anyhow::Result<Vec<Vec<u8>>> {
        let mut result = vec![];
        for album in &self.albums {
            let (id, _) = album?;
            result.push(id.to_vec());
        }
        Ok(result)
    }

    pub fn load_albums_hash(&mut self) -> anyhow::Result<()> {
        let mut result = vec![];
        for album in &self.albums {
            let (_, album) = album?;
            let album = rkyv::from_bytes::<Album, Error>(&album)?;
            result.push(album);
        }
        self.db_hash.albums_hash = result;
        Ok(())
    }

    pub fn clear_albums(&self) -> anyhow::Result<()> {
        self.albums.clear()?;
        self.albums.flush()?;
        Ok(())
    }
}

#[test]
fn test_db_tracks() -> anyhow::Result<()> {
    use crate::utils::get_argument_val;
    use anyhow::bail;
    use std::path::PathBuf;

    let mut db = DB::open()?;

    let path_to_file = get_argument_val("FILE_PATH").unwrap();

    let path_to_file = PathBuf::from(path_to_file);

    if let Err(err) = db.load_tracks_hash() {
        bail!(err)
    }

    match Track::create_tracks_list_from_file(path_to_file, &db) {
        std::result::Result::Ok(tracks) => {
            for track in tracks {
                db.save_track(track)?;
            }
        }
        Err(err) => {
            logger::warn(err);
        }
    }

    if let Err(err) = db.load_tracks_hash() {
        bail!(err)
    }

    for track in db.db_hash.tracks_hash {
        logger::debug(format!("{}", track));
    }

    Ok(())
}

#[test]
fn test_db_albums() -> anyhow::Result<()> {
    let mut db = DB::open()?;

    db.load_tracks_hash()?;
    db.load_albums_hash()?;

    let tracks_hash = &db.db_hash.tracks_hash;
    let albums_hash = &db.db_hash.albums_hash;

    for i in tracks_hash {
        logger::debug(i);
    }

    let load_albums = Album::group_tracks(albums_hash, tracks_hash)?;

    for album in load_albums {
        db.save_album(album)?;
    }

    for album in albums_hash {
        logger::debug(format!("from hash: {}", album));
    }

    Ok(())
}
#[test]
fn test_clear_db() -> anyhow::Result<()> {
    let db = DB::open()?;
    db.clear_tracks()?;
    db.clear_albums()?;
    Ok(())
}
