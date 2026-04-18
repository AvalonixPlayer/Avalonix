use std::{
    collections::HashSet,
    fs::{File, OpenOptions},
    io::{Read, Write},
    path::{Path, PathBuf},
};

use anyhow::Ok;
use serde::{Deserialize, Serialize};

use crate::{disk::disk_manager, utils::get_argument_val};

#[derive(Serialize, Deserialize, Debug)]
pub struct Settings {
    pub lib_paths: HashSet<PathBuf>,
}

impl Settings {
    pub fn open() -> anyhow::Result<Self> {
        let path = disk_manager::settings_path();

        let mut file = File::open(path)?;

        let mut text = String::new();
        file.read_to_string(&mut text)?;

        let result;

        if let Result::Ok(data) = serde_json::from_str::<Settings>(&text) {
            result = data;
        } else {
            result = Self::new();
        }

        Ok(result)
    }

    fn new() -> Self {
        Self {
            lib_paths: HashSet::new(),
        }
    }

    pub fn save(&self) -> anyhow::Result<()> {
        let path = disk_manager::settings_path();

        let mut file = File::create(path)?;

        let data = serde_json::to_string(self)?;

        file.write_all(data.as_bytes())?;

        Ok(())
    }

    pub fn add_lib_path(&mut self, path: &PathBuf) {
        self.lib_paths.insert(path.clone());
    }
}

#[test]
fn test_settings() -> anyhow::Result<()> {
    use crate::logger;
    let mut settings = Settings::open()?;

    let lib_path = get_argument_val("LIB_PATH");

    if let None = lib_path {
        return Ok(());
    }

    let lib_path = lib_path.unwrap();

    logger::debug(format!("Opened settings: {:#?}", settings));

    settings.add_lib_path(&PathBuf::from(lib_path));

    settings.save();

    logger::debug(format!("Saved settings: {:#?}", settings));

    Ok(())
}
