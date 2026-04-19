use std::{fmt::Display, io::Cursor, path::Path};

use anyhow::{Ok, bail};
use infer::MatcherType;

use crate::{
    media::audio_file::{AudioFile, CUEFile, SingleFile},
    metadata::track_metadata::TrackMetadata,
};

pub struct Track {
    pub metadata: TrackMetadata,
}

impl Track {
    pub fn create_tracks_list_from_file<P: AsRef<Path>>(path: P) -> anyhow::Result<Vec<Track>> {
        match infer::get_from_path(&path) {
            std::result::Result::Ok(kind) => match kind {
                Some(kind) => match kind.matcher_type() {
                    MatcherType::Audio => {
                        let tracks_metadatas = SingleFile::read_metadatas(path)?;

                        let mut result = vec![]; // I decided to play it safe and still make it a cycle, paranoia
                        for tm in tracks_metadatas {
                            result.push(Self { metadata: tm });
                        }
                        return Ok(result);
                    }
                    MatcherType::Text => {
                        let tracks_metadatas = CUEFile::read_metadatas(path)?;

                        let mut result = vec![];
                        for tm in tracks_metadatas {
                            result.push(Self { metadata: tm });
                        }
                        return Ok(result);
                    }
                    _ => {
                        bail!("file not cue and not audio")
                    }
                },
                None => {
                    bail!("Can`t to read file type")
                }
            },
            Err(_) => {
                bail!("Can`t to read file type")
            }
        }
    }

    pub fn get_data(&self) -> anyhow::Result<Cursor<Vec<u8>>> {
        let cursor = Cursor::new(vec![]);
        Ok(cursor)
    }
}

impl Display for Track {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "\ttrack_path: {}\n\ttitle: {}",
            self.metadata.file_path, self.metadata.title
        )
    }
}

#[test]
fn test_create_tracks() -> anyhow::Result<()> {
    use walkdir::WalkDir;

    use crate::{disk::settings::Settings, logger};

    let settings = Settings::open()?;

    for lib_path in settings.lib_paths {
        for dir_entry in WalkDir::new(lib_path) {
            let dir_entry = dir_entry?;

            if dir_entry.file_type().is_file() {
                let path = dir_entry.path();

                println!("{}", path.to_str().unwrap());

                match Track::create_tracks_list_from_file(path) {
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
