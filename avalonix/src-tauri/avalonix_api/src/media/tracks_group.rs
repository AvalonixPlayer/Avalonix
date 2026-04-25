use crate::media::track::Track;

pub trait TracksGroup {
    type Output;
    fn group_tracks(
        selfs_hash: &Vec<Self::Output>,
        tracks_hash: &Vec<Track>,
    ) -> anyhow::Result<Vec<Self::Output>>;
}
