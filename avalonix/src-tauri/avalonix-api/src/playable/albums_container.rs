use std::collections::HashMap;

use crate::{
    db::MusicDB,
    disk_manager, logger,
    playable::{
        album::{self, Album, AlbumMetadata},
        library_part::LibraryPart,
        playboxes::UpdateLib,
        tracks_container::TracksContainer,
    },
    settings_manager::Settings,
};

#[derive(ts_rs::TS)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\AlbumsContainer.ts")]
pub struct AlbumsContainer {
    pub all_albums_ids: Vec<Vec<u8>>,
}

impl LibraryPart for AlbumsContainer {
    type Output = Album;

    fn fill_ids(&mut self, db: &MusicDB) {
        let ids = db.get_all_albums_id().unwrap_or_else(|err| {
            logger::error(&err.to_string());
            Vec::new()
        });

        self.all_albums_ids = ids;
    }

    fn get_by_id(&self, db: &MusicDB, id: Vec<u8>) -> anyhow::Result<Self::Output> {
        let album = db.get_album_by_id(&id)?;
        Ok(album)
    }
}

impl AlbumsContainer {
    pub fn new() -> Self {
        AlbumsContainer {
            all_albums_ids: Vec::new(),
        }
    }
}

impl UpdateLib for AlbumsContainer {
    fn update_lib(&self, db: &MusicDB, _settings: &Settings) {
        if let Ok(albums_hash) = db.get_all_albums() {
            if let Ok(tracks) = db.get_all_tracks() {
                let mut albums: HashMap<String, Album> = HashMap::new();
                for track in tracks {
                    if let Some(album) = albums.get_mut(track.metadata.album.as_ref().unwrap()) {
                        let track_id = track.id.as_bytes().to_vec();

                        if !album.tracks_ids.contains(&track_id) {
                            album.tracks_ids.push(track_id);
                        }
                    } else {
                        let vec = vec![track.id.as_bytes().to_vec()];

                        let album = Album::new(db, &vec, &albums_hash);
                        let name = track.metadata.album.as_ref().unwrap();
                        albums.insert(name.to_string(), album);
                    }
                }

                for album in albums {
                    _ = db.save_album(&album.1);
                }
            }
        }
    }
}

#[test]
fn test_albums_container() {
    let settings = Settings::new().unwrap();
    let db_path = &disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(db_path).unwrap();

    let container = AlbumsContainer::new();

    container.update_lib(&db, &settings);

    let albums_ids = db.get_all_albums_id().unwrap();

    for id in albums_ids {
        let album = container.get_by_id(&db, id);

        match album {
            Ok(album) => {
                logger::debug(&format!("{}", album.metadata.name));
            }
            Err(_) => {}
        }
    }
}
