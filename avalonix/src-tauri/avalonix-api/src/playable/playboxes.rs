use crate::{
    db::MusicDB,
    playable::{
        albums_container::AlbumsContainer, artists_container::AristsContainer,
        library_part::LibraryPart, tracks_container::TracksContainer,
    },
    settings_manager::Settings,
};

pub struct PlayboxesManager {
    pub tracks_container: TracksContainer,

    pub albums_container: AlbumsContainer,
    /*
    pub artists_container: AristsContainer,
     */
}

impl PlayboxesManager {
    pub fn new(
        tracks_container: TracksContainer,

        albums_container: AlbumsContainer,
        /*
        artists_container: AristsContainer,
         */
    ) -> PlayboxesManager {
        PlayboxesManager {
            tracks_container,
            albums_container,
            /*
            artists_container,
             */
        }
    }

    pub fn update_lib(&mut self, db: &MusicDB, settings: &Settings) {
        self.tracks_container.update_lib(db, settings);
        self.tracks_container.fill_ids(db);

        self.albums_container.update_lib(db, settings);
        self.albums_container.fill_ids(db);
    }
}

pub trait UpdateLib {
    fn update_lib(&self, db: &MusicDB, settings: &Settings);
}
