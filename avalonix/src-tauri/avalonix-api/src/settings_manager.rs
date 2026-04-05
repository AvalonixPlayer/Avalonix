use serde::{Deserialize, Serialize};
use std::fs;
use std::io;
use crate::disk_manager::avalonix_special_folder_path;

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Settings {
    pub volume: u8,
    pub library_paths: Vec<String>,
    pub equalizer: Equalizer,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Equalizer {
    pub master: u8,
    pub eq_stats: Vec<u8>,
}


pub type SettingsResult<T> = Result<T, SettingsError>;

#[derive(Debug)]
pub enum SettingsError {
    IoError(io::Error),
    JsonError(serde_json::Error),
}

impl From<io::Error> for SettingsError {
    fn from(err: io::Error) -> Self {
        SettingsError::IoError(err)
    }
}

impl From<serde_json::Error> for SettingsError {
    fn from(err: serde_json::Error) -> Self {
        SettingsError::JsonError(err)
    }
}

pub fn save_settings(st: &Settings) -> SettingsResult<()> {
    let json = serde_json::to_string_pretty(st)?;
    fs::write(avalonix_special_folder_path() + "settings.json", json)?;
    Ok(())
}

pub fn read_settings() -> SettingsResult<Settings> {
    let content = fs::read_to_string(avalonix_special_folder_path() + "settings.json")?;
    let settings: Settings = serde_json::from_str(&content)?;
    Ok(settings)
}

pub fn default_equalizer() -> Equalizer {
    Equalizer { master: 50, eq_stats: vec![50, 50, 50, 50, 50, 50, 50, 50] }
}

pub fn default_settings() -> Settings {
    Settings {
        volume: 50,
        library_paths: vec![],
        equalizer: default_equalizer(),
    }
}

pub fn read_or_create_settings() -> SettingsResult<Settings> {
    match read_settings() {
        Ok(settings) => Ok(settings),
        Err(_) => {
            let default = default_settings();
            save_settings(&default)?;
            Ok(default)
        }
    }
}

    
#[test]
fn test_save_and_read_settings() {
    let settings = Settings {
        volume: 75,
        library_paths: vec![String::from("/mnt/music")],
    };

    save_settings(&settings).expect("Failed to save");
    let loaded = read_settings().expect("Failed to read");

    assert_eq!(settings.volume, loaded.volume);
    assert_eq!(settings.library_paths, loaded.library_paths);
}
