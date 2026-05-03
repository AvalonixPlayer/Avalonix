use std::{
    fs,
    sync::{Arc, Mutex},
    time::UNIX_EPOCH,
};

use anyhow::Ok;
use rkyv::rancor::Error;

use crate::{
    disk::{
        disk_manager,
        settings::Settings,
    },
    logger,
    media::{album::Album, performer::Performer, track::Track, tracks_group::TracksGroup},
    metadata::{
        album_filter_metadata::AlbumFilterMetadata, filter_metadata::FilterMetadata,
        performer_filter_metadata::PerformerFilterMetadata,
        track_filter_metadata::TrackFilterMetadata,
    },
    mutex_work::CreateArcMutex,
};

pub struct DB {
    pub db_hash: DBHash,
    tracks: sled::Tree,
    albums: sled::Tree,
    performers: sled::Tree,
}

#[derive(Clone)]
pub struct DBHash {
    pub tracks_hash: Vec<Track>,
    pub albums_hash: Vec<Album>,
    pub performers_hash: Vec<Performer>,
}

impl DBHash {
    pub fn new() -> Self {
        Self {
            tracks_hash: vec![],
            albums_hash: vec![],
            performers_hash: vec![],
        }
    }
}

impl DB {
    pub fn open() -> anyhow::Result<Arc<Mutex<Self>>> {
        let db_path = disk_manager::db_path();
        let db = sled::open(db_path)?;

        let tracks_tree = db.open_tree(b"tracks")?;
        let albums_tree = db.open_tree(b"albums")?;
        let performers_tree = db.open_tree(b"performers")?;

        let db = Self {
            db_hash: DBHash::new(),
            tracks: tracks_tree,
            albums: albums_tree,
            performers: performers_tree,
        };

        Ok(db.create_arc_mutex())
    }

    pub fn update_library(&mut self, settings: &Settings) -> anyhow::Result<()> {
        self.load_tracks_hash()?;
        self.load_albums_hash()?;
        self.load_performers_hash()?;

        self.update_tracks_library(settings)?;
        self.update_albums_library()?;
        self.update_performers_library()?;

        self.load_tracks_hash()?;
        self.load_albums_hash()?;
        self.load_performers_hash()?;
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
        logger::debug("Update tracks lib");

        for track_file_path in tracks_files_paths {
            if let Some(track_in_lib) = self
                .db_hash
                .tracks_hash
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

    pub fn update_albums_library(&mut self) -> anyhow::Result<()> {
        logger::debug("Update albums lib");

        let tracks = &self.db_hash.tracks_hash;
        let albums = &self.db_hash.albums_hash;
        let new_albums = Album::group_tracks(albums, tracks)?;

        for album in new_albums {
            if let Some(exists_album) = albums.iter().find(|a| a.tracks_ids == album.tracks_ids) {
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
    pub fn save_performer(&self, performer: Performer) -> anyhow::Result<()> {
        let key = &performer.performer_metadata.id;

        let value_bytes = rkyv::to_bytes::<Error>(&performer)?.to_vec();

        self.performers.insert(key, value_bytes)?;
        self.performers.flush()?;

        logger::info(format!("performer saved to db: {}", performer));

        Ok(())
    }

    pub fn get_performers_ids(&self) -> anyhow::Result<Vec<Vec<u8>>> {
        let mut result = vec![];
        for performer in &self.performers {
            let (id, _) = performer?;
            result.push(id.to_vec());
        }
        Ok(result)
    }

    pub fn load_performers_hash(&mut self) -> anyhow::Result<()> {
        let mut result = vec![];
        for performer in &self.performers {
            let (_, performer) = performer?;
            let performer = rkyv::from_bytes::<Performer, Error>(&performer)?;
            result.push(performer);
        }
        self.db_hash.performers_hash = result;
        Ok(())
    }

    pub fn clear_performers(&self) -> anyhow::Result<()> {
        self.performers.clear()?;
        self.performers.flush()?;
        Ok(())
    }

    pub fn update_performers_library(&mut self) -> anyhow::Result<()> {
        logger::debug("Update albums lib");

        let tracks = &self.db_hash.tracks_hash;
        let performers = &self.db_hash.performers_hash;
        let new_performers = Performer::group_tracks(performers, tracks)?;

        for performer in new_performers {
            if let Some(exists_performer) = performers
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

#[test]
fn test_db_tracks() -> anyhow::Result<()> {
    use crate::utils::get_argument_val;
    use anyhow::bail;
    use std::path::PathBuf;

    let db = DB::open()?;

    let path_to_file = get_argument_val("FILE_PATH").unwrap();

    let path_to_file = PathBuf::from(path_to_file);

    let mut db_guard = db.lock().unwrap();

    if let Err(err) = db_guard.load_tracks_hash() {
        bail!(err)
    }

    match Track::create_tracks_list_from_file(path_to_file, &db_guard) {
        std::result::Result::Ok(tracks) => {
            for track in tracks {
                db_guard.save_track(track)?;
            }
        }
        Err(err) => {
            logger::warn(err);
        }
    }

    if let Err(err) = db_guard.load_tracks_hash() {
        bail!(err)
    }

    for track in &db_guard.db_hash.tracks_hash {
        logger::debug(format!("{}", track));
    }

    Ok(())
}

#[test]
fn test_db_albums() -> anyhow::Result<()> {
    let db = DB::open()?;

    let settings = &Settings::open()?;

    let mut db_guard = db.lock().unwrap();

    db_guard.update_library(settings)?;

    let albums_hash = &db_guard.db_hash.albums_hash;

    for album in albums_hash {
        logger::debug(format!("from hash: {}", album));
        for track_id in &album.tracks_ids {
            logger::debug(format!("\ttrack id: {:?}", track_id));
        }
    }

    Ok(())
}

#[test]
fn test_db_performers() -> anyhow::Result<()> {
    let db = DB::open()?;

    let settings = &Settings::open()?;

    let mut db_guard = db.lock().unwrap();

    db_guard.update_library(settings)?;

    let performers_hash = &db_guard.db_hash.performers_hash;

    for performer in performers_hash {
        logger::debug(format!("from hash: {}", performer));
        for track_id in &performer.tracks_ids {
            logger::debug(format!("\ttrack id: {:?}", track_id));
        }
    }

    Ok(())
}

#[test]
fn test_db_update() -> anyhow::Result<()> {
    let db = DB::open()?;

    let mut db_guard = db.lock().unwrap();

    let settings = &Settings::open()?;
    db_guard.update_library(settings)?;

    Ok(())
}

#[test]
fn test_clear_db() -> anyhow::Result<()> {
    let db = DB::open()?;

    let db_guard = db.lock().unwrap();
    db_guard.clear_tracks()?;
    db_guard.clear_albums()?;
    Ok(())
}
