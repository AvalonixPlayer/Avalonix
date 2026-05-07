use std::{
    collections::HashSet,
    fs::File,
    io::{Read, Write},
};

use anyhow::Ok;
use serde::{Deserialize, Serialize};

use crate::disk::disk_manager;

/// User settings structure
#[derive(Serialize, Deserialize, Debug)]
pub struct Settings {
    /// Paths to folders with tracks
    pub lib_paths: HashSet<String>,
}

impl Settings {
    /// Creates or opens a settings
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

    /// Saves settings
    pub fn save(&self) -> anyhow::Result<()> {
        let path = disk_manager::settings_path();

        let mut file = File::create(path)?;

        let data = serde_json::to_string(self)?;

        file.write_all(data.as_bytes())?;

        Ok(())
    }

    /// Adds the path to the folder with tracks to the settings
    pub fn add_lib_path<T: AsRef<str>>(&mut self, path: T) {
        self.lib_paths.insert(path.as_ref().to_string());
    }

    /// Removes the path to the folder with tracks to the settings
    pub fn remove_path<T: AsRef<str>>(&mut self, path: T) {
        self.lib_paths.remove(path.as_ref());
    }
}
