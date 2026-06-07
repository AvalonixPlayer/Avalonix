use std::{
    env,
    fs::{self, File},
    path::PathBuf,
};

use anyhow::Result;

pub(crate) fn avalonix_foled_path() -> Result<PathBuf> {
    match env::home_dir() {
        Some(path) => {
            let path = path.join(".avalonix/");
            if !fs::exists(&path)? {
                fs::create_dir(&path)?;
                return Ok(path);
            }

            let metadata = fs::metadata(&path)?;
            if !metadata.is_dir() {
                fs::remove_file(&path)?;
                fs::create_dir(&path)?;
            }

            return Ok(path);
        }
        None => {
            panic!("user path don`t exists");
        }
    }
}

pub(crate) fn avalonix_db() -> Result<PathBuf> {
    Ok(avalonix_foled_path()?.join("db/"))
}

pub(crate) fn settings_path() -> Result<PathBuf> {
    let path = avalonix_foled_path()?.join("settings.json");
    if !path.exists() {
        File::create_new(&path)?;
    }
    Ok(path)
}
