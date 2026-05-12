use std::sync::{Arc, Mutex};

use avalonix_api::disk::settings::Settings;
use better_sms::mutex::{MutexGuardWork, MutexWork};

#[tauri::command]
pub async fn get_library_folders_from_settings(
    settings: tauri::State<'_, Arc<Mutex<Settings>>>,
) -> Result<Vec<String>, String> {
    settings.lock_unw().use_guard(|guard| {
        let mut res = vec![];
        for i in &guard.lib_paths {
            res.push(i.clone());
        }
        Ok(res)
    })
}

#[tauri::command]
pub async fn add_folder_path_to_library(
    settings: tauri::State<'_, Arc<Mutex<Settings>>>,
    path: String,
) -> Result<(), String> {
    settings.lock_unw().use_guard(|guard| {
        guard.add_lib_path(path);
        guard.save().map_err(|err| err.to_string())
    })
}

#[tauri::command]
pub async fn remove_folder_path_from_library(
    settings: tauri::State<'_, Arc<Mutex<Settings>>>,
    path: String,
) -> Result<(), String> {
    settings.lock_unw().use_guard(|guard| {
        guard.remove_path(path);
        guard.save().map_err(|err| err.to_string())
    })
}
