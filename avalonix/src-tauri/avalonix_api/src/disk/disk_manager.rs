use std::{
    collections::HashSet,
    fs::{self, File},
    path::PathBuf,
};

use glob::glob;

use crate::{disk::settings::Settings, logger};

pub fn avalonix_folder_path() -> PathBuf {
    let res = dirs::home_dir().unwrap().join(".avalonix");
    check_dir(&res);
    res
}

pub fn lib_db_path() -> PathBuf {
    let res = avalonix_folder_path().join("lib_db");
    check_dir(&res);
    res
}

pub fn settings_path() -> PathBuf {
    let res = avalonix_folder_path().join("settings.json");
    check_file(&res);
    res
}

pub fn db_path() -> PathBuf {
    let res = avalonix_folder_path().join("db");
    check_dir(&res);
    res
}

pub fn get_tracks_files_paths(settings: &Settings) -> Vec<PathBuf> {
    let mut result: HashSet<PathBuf> = HashSet::new();

    for folder in &settings.lib_paths {
        for entry in glob(&format!("{}/**/*", folder.to_str().unwrap().to_string()))
            .expect("Can`t to read glob")
        {
            match entry {
                Ok(path) => _ = result.insert(path),
                Err(err) => _ = logger::error(err),
            }
        }
    }

    result.into_iter().collect()
}

fn check_dir(dir: &PathBuf) {
    if !dir.exists() {
        _ = fs::create_dir(&dir);
    } else if dir.is_file() {
        _ = fs::remove_file(&dir);
        _ = fs::create_dir(&dir);
    }
}

fn check_file(file: &PathBuf) {
    if !file.exists() {
        _ = File::create(&file);
    } else if file.is_dir() {
        _ = fs::read_dir(&file);
        _ = File::create(&file);
    }
}

#[test]
fn test_disk_manager() {
    use crate::logger;
    logger::debug(avalonix_folder_path().to_str().unwrap().to_string());
    logger::debug(lib_db_path().to_str().unwrap().to_string());
}
