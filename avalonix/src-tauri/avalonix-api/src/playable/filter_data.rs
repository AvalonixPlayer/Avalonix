#[derive(ts_rs::TS, serde::Serialize)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\FilterData.ts")]
pub struct FilterData {
    pub id: Vec<u8>,
    pub name: String,
}

pub trait FilterDataTrait {
    fn new_dilter_data(&self, id: Vec<u8>) -> FilterData;
}
