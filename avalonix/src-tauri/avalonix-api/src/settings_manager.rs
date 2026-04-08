use crate::disk_manager::avalonix_settings_path;
use crate::logger;
use serde::{Deserialize, Serialize};
use std::fs;
use std::io;

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

impl Settings {
    pub fn new() -> anyhow::Result<Self> {
        Self::read_or_create_settings()
    }

    pub fn save_settings(&self) -> anyhow::Result<()> {
        let json = serde_json::to_string_pretty(self)?;
        fs::write(avalonix_settings_path(), json)?;
        Ok(())
    }

    fn read_settings() -> anyhow::Result<Self> {
        let content = fs::read_to_string(avalonix_settings_path())?;
        let settings: Settings = serde_json::from_str(&content)?;
        Ok(settings)
    }

    fn default_equalizer() -> Equalizer {
        Equalizer {
            master: 50,
            eq_stats: vec![50, 50, 50, 50, 50, 50, 50, 50],
        }
    }

    fn default_settings() -> Settings {
        Settings {
            volume: 50,
            library_paths: vec![],
            equalizer: Self::default_equalizer(),
        }
    }

    fn read_or_create_settings() -> anyhow::Result<Self> {
        match Self::read_settings() {
            Ok(settings) => Ok(settings),
            Err(_) => {
                let default = Self::default_settings();
                Self::save_settings(&default)?;
                Ok(default)
            }
        }
    }
}

#[test]
fn test_save_and_read_settings() {
    use crate::logger;
    use crate::utils::get_argument_val;

    if let Some(lib_path) = get_argument_val("LIB_PATH") {
        let settings = Settings::new();

        match settings {
            Ok(mut settings) => {
                settings.library_paths = vec![lib_path];

                settings.save_settings().expect("Failed to save");
            }
            Err(err) => {
                logger::error(&err.to_string());
            }
        }
    }
}
