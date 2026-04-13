use anyhow::Ok;

pub struct Api {}

impl Api {
    pub fn new() -> anyhow::Result<Self> {
        Ok(Self {})
    }
}

pub fn init_api() -> anyhow::Result<Api> {
    Api::new()
}
