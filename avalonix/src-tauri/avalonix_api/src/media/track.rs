use std::{fmt::Display, io::Cursor, path::Path};

use anyhow::{Ok, bail};
use rkyv::{Archive, Deserialize, Serialize};
use uuid::Uuid;

use crate::{
    disk::db::{self, DB},
    media::audio_file::{AudioFile, CUEFile, LibFile, LibFileTrait, SingleFile},
    metadata::track_metadata::TrackMetadata,
};

#[derive(Archive, Debug, Deserialize, Serialize, Clone)]
pub struct Track {
    pub metadata: TrackMetadata,
}

impl Track {
    pub fn create_new(metadata: TrackMetadata) -> Self {
        Self { metadata }
    }

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
                        result.push(Self::create_new(tm));
                    }
                }
                LibFile::Cue => {
                    let metadatas = CUEFile::read_metadatas(&path, db)?;
                    for tm in metadatas {
                        result.push(Self::create_new(tm));
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

    pub fn get_data(&self) -> anyhow::Result<Cursor<Vec<u8>>> {
        let cursor = Cursor::new(vec![]);
        Ok(cursor)
    }
}

impl Display for Track {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "\t\t{}", self.metadata)
    }
}

#[test]
fn test_create_tracks() -> anyhow::Result<()> {
    use walkdir::WalkDir;

    use crate::{disk::settings::Settings, logger};

    let settings = Settings::open()?;

    let db = &DB::open()?;

    for lib_path in settings.lib_paths {
        for dir_entry in WalkDir::new(lib_path) {
            let dir_entry = dir_entry?;

            if dir_entry.file_type().is_file() {
                let path = dir_entry.path();

                match Track::create_tracks_list_from_file(path, db) {
                    std::result::Result::Ok(tracks) => {
                        for track in tracks {
                            logger::debug(format!("track readed: {}", track));
                        }
                    }
                    Err(err) => {
                        logger::warn(err);
                    }
                }
            }
        }
    }
    Ok(())
}
