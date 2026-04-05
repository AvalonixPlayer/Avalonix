use crate::{media::track::Track, playboxes::album::Album, utils::get_argument_val};
use sled::{Error as SledError, Tree};

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

    pub fn get_size_on_disk(&self) -> u64 {
        self.db.size_on_disk().unwrap_or(0)
    }
}

#[test]
fn test_db() {
    use crate::disk_manager;
    use crate::logger;

    let hash_path = disk_manager::avalonix_special_folder_path();

    let music_path = get_argument_val(&"TRACK_PATH");

    let Some(_) = music_path else {
        return;
    };

    let db = MusicDB::open(&hash_path);
    match db {
        Ok(db) => {
            let all_tracks = db.get_all_tracks().unwrap();
            let tracks_hash = all_tracks.iter().collect();

            let track = Track::new(&music_path.unwrap(), &db, tracks_hash).unwrap();

            _ = db.save_track(&track);

            let tracks = db.get_all_tracks().unwrap();
            for track in tracks {
                logger::debug(&format!("{}", track.metadata));
            }
        }
        Err(err) => logger::error(&err.to_string()),
    }
}
