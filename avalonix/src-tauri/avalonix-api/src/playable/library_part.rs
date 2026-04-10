use crate::db::MusicDB;

pub trait LibraryPart {
    type Output;
    fn get_by_id(&self, db: &MusicDB, id: Vec<u8>) -> anyhow::Result<Self::Output>;

    fn fill_ids(&mut self, db: &MusicDB);
}
