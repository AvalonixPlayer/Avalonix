use std::fs;

use anyhow::Result;
use serde::{Deserialize, Serialize};

use crate::disk::{disk_paths::settings_path, user::theme::Theme};

#[derive(Deserialize, Serialize)]
pub struct UserSettings {
    pub library_paths: Vec<String>,
    pub themes: Vec<Theme>,
    pub active_theme: Theme,
}

impl UserSettings {
    pub fn open() -> Result<Self> {
        if let Ok(result) = Self::try_read_from_save() {
            return Ok(result);
        }
        let theme = Theme::new();

        let settings = Self {
            library_paths: vec![],
            themes: vec![theme.clone()],
            active_theme: theme,
        };
        Ok(settings)
    }

    pub fn save(&self) -> Result<()> {
        fs::write(&settings_path()?, serde_json::to_string(&self)?)?;
        Ok(())
    }

    fn try_read_from_save() -> Result<Self> {
        Ok(serde_json::from_str(
            &fs::read_to_string(settings_path()?)?,
        )?)
    }
}
