use serde::Serialize;

#[derive(Serialize, ts_rs::TS, Debug)]
#[ts(export)]
pub struct PerformerFilterMetadata {
    pub id: Vec<u8>,
    pub title: String,
}
