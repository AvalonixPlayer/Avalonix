use std::{fmt::Display, fs, io::Cursor, path::Path, time::UNIX_EPOCH};

use anyhow::{Ok, bail};
use rkyv::{Archive, Deserialize, Serialize};

use crate::{
    disk::db::DB,
    logger,
    media::audio_file::{AudioFile, CUEFile, LibFile, LibFileTrait, SingleFile},
    metadata::track_metadata::TrackMetadata,
};

#[derive(Archive, Debug, Deserialize, Serialize, Clone)]
pub struct Track {
    pub start_file_path: String,
    pub metadata: TrackMetadata,
    pub mod_date: u64,
}

impl Track {
    /// Creates a new track
    pub fn create_new(start_file_path: String, metadata: TrackMetadata) -> anyhow::Result<Self> {
        let f = fs::metadata(&start_file_path)?;
        let mod_date;
        match f.modified() {
            Result::Ok(md) => {
                mod_date = md.duration_since(UNIX_EPOCH).unwrap().as_secs();
            }
            Err(_) => {
                mod_date = UNIX_EPOCH.duration_since(UNIX_EPOCH).unwrap().as_secs();
            }
        };
        Ok(Self {
            start_file_path,
            metadata,
            mod_date,
        })
    }

    /// Creates a new tracks list from file
    pub fn create_tracks_list_from_file<P: AsRef<Path>>(
        path: P,
        db: &DB,
    ) -> anyhow::Result<Vec<Track>> {
        let push_all = |tp: LibFile| {
            let mut result = vec![];

            match tp {
                LibFile::Audio => {
                    let metadatas = SingleFile::read_metadatas(&path, db)?;
                    for tm in metadatas {
                        result.push(Self::create_new(
                            path.as_ref().as_os_str().to_str().unwrap().to_string(),
                            tm,
                        )?);
                    }
                }
                LibFile::Cue => {
                    let metadatas = CUEFile::read_metadatas(&path, db)?;
                    for tm in metadatas {
                        result.push(Self::create_new(
                            path.as_ref().as_os_str().to_str().unwrap().to_string(),
                            tm,
                        )?);
                    }
                }
                _ => bail!("Can`t to read file"),
            }

            Ok(result)
        };

        match path.file_type() {
            super::audio_file::LibFile::Audio => Ok(push_all(LibFile::Audio)?),
            super::audio_file::LibFile::Cue => Ok(push_all(LibFile::Cue)?),
            super::audio_file::LibFile::NotForLib => bail!("file not for lib"),
        }
    }

    /// Returns audio data to the media player
    pub fn get_data(&self) -> anyhow::Result<Cursor<Vec<u8>>> {
        let cursor = Cursor::new(fs::read(&self.metadata.file_path)?);
        Ok(cursor)
    }

    /// Returns the track's cover in string format.
    pub fn get_cover_as_uri(&self) -> anyhow::Result<String> {
        match self.start_file_path.file_type() {
            LibFile::Audio => SingleFile::get_cover_as_uri(&self.start_file_path),
            LibFile::Cue => CUEFile::get_cover_as_uri(&self.start_file_path),
            LibFile::NotForLib => {
                _ = logger::debug("it`s imposible");
                bail!("it`s imposible")
            }
        }
    }
}

impl Display for Track {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "\n\t\tstart path: {}\n\t\t{}",
            self.start_file_path, self.metadata
        )
    }
}
