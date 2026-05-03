use std::fmt::Display;

use rkyv::{Archive, Deserialize, Serialize};

use crate::{
    media::{performer::Performer, track::Track},
    metadata::{
        filter_metadata::FilterMetadata, performer_filter_metadata::PerformerFilterMetadata,
    },
};

#[derive(Debug, Archive, Serialize, Deserialize, Clone)]
pub struct PerformerMetadata {
    pub id: Vec<u8>,
    pub performer_title: String,
}

impl PerformerMetadata {
    pub fn from(id: &Vec<u8>, track: &Track, performers_hash: &Vec<Performer>) -> Self {
        if let Some(performer) = performers_hash
            .iter()
            .find(|x| x.performer_metadata.performer_title == track.metadata.artist)
        {
            return performer.performer_metadata.clone();
        }
        Self {
            id: id.clone(),
            performer_title: track.metadata.artist.clone(),
        }
    }
}

impl FilterMetadata for PerformerMetadata {
    type Output = PerformerFilterMetadata;
    fn get_filter_metadata(&self) -> anyhow::Result<Self::Output> {
        let result = PerformerFilterMetadata {
            id: self.id.clone(),
            title: self.performer_title.clone(),
        };
        Ok(result)
    }
}

impl Display for PerformerMetadata {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "\n\t\tid: {:?}\n\t\ttitle: {}",
            self.id, self.performer_title
        )
    }
}
