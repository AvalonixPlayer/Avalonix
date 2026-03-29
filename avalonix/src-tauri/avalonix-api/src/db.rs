use crate::media::track::Track;
use rkyv::{self};
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

        let bytes = rkyv::to_bytes::<rkyv::rancor::Error>(track).map_err(to_sled_error)?;

        self.tracks.insert(key, bytes.as_ref())?;

        if let Some(artist) = &track.metadata.artist {
            self.artists.insert(artist.as_bytes(), key)?;
        }
        if let Some(album) = &track.metadata.album {
            self.albums.insert(album.as_bytes(), key)?;
        }

        self.tracks.flush()?;
        Ok(())
    }

    pub fn get_track(&self, id: &str) -> sled::Result<Option<Track>> {
        match self.tracks.get(id.as_bytes())? {
            Some(value) => {
                let track: Track = rkyv::from_bytes::<Track, rkyv::rancor::Error>(&value)
                    .map_err(to_sled_error)?;
                Ok(Some(track))
            }
            None => Ok(None),
        }
    }

    pub fn get_all_tracks(&self) -> sled::Result<Vec<Track>> {
        let mut tracks = Vec::new();
        for item in self.tracks.iter() {
            let (_, value) = item?;
            let track: Track =
                rkyv::from_bytes::<Track, rkyv::rancor::Error>(&value).map_err(to_sled_error)?;
            tracks.push(track);
        }
        Ok(tracks)
    }

    pub fn delete_track(&self, id: &str) -> sled::Result<()> {
        self.tracks.remove(id.as_bytes())?;
        Ok(())
    }

    pub fn get_size_on_disk(&self) -> u64 {
        self.db.size_on_disk().unwrap_or(0)
    }
}

#[test]
fn test_db() {
    use crate::disk_manager;
    use crate::logger;
    use crate::media::metadata::Metadata;

    let hash_path = disk_manager::avalonix_special_folder_path();

    let music_path =
        "D:\\music\\Three Days Grace [restored]\\2006 - One-X\\03. Animal I Have Become.flac";

    let db = MusicDB::open(&hash_path);
    match db {
        Ok(db) => {
            let all_tracks = db.get_all_tracks().unwrap();
            let tracks_hash = all_tracks.iter().collect();

            let track = Track::new(music_path, &db, tracks_hash).unwrap();

            _ = db.save_track(&track);

            let tracks = db.get_all_tracks().unwrap();
            for track in tracks {
                logger::debug(&format!("{}", track.metadata));
            }
        }
        Err(err) => logger::error(&err.to_string()),
    }
}
