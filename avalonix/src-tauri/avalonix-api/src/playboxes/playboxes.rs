use crate::playboxes::{
    albums_container::AlbumsContainer, artists_container::AristsContainer,
    tracks_container::TracksContainer,
};

pub struct PlayboxesManager {
    pub tracks_container: TracksContainer,
    pub albums_container: AlbumsContainer,
    pub artists_container: AristsContainer,
}

impl PlayboxesManager {
    pub fn new(
        tracks_container: TracksContainer,
        albums_container: AlbumsContainer,
        artists_container: AristsContainer,
    ) -> PlayboxesManager {
        PlayboxesManager {
            tracks_container,
            albums_container,
            artists_container,
        }
    }
}
