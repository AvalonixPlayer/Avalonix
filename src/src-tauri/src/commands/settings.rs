use std::{
    ops::DerefMut,
    sync::{Arc, RwLock},
};

use avalonix_api::disk::user::settings::UserSettings;

#[tauri::command]
pub async fn get_library_paths(
    settings: tauri::State<'_, Arc<RwLock<UserSettings>>>,
) -> Result<Vec<String>, String> {
    Ok(settings.read().unwrap().library_paths.clone())
}

#[tauri::command]
pub async fn remove_path_from_lib(
    settings: tauri::State<'_, Arc<RwLock<UserSettings>>>,
    path: String,
) -> Result<(), String> {
    let mut guard = settings.write().unwrap();
    if let Some(pos) = guard.library_paths.iter().position(|p| *p == path) {
        guard.library_paths.remove(pos);
        guard.save().map_err(|err| err.to_string())?;
        guard.path_removed = true;
    } else {
        return Err("Path don`t in lib".to_string());
    }

    Ok(())
}

#[tauri::command]
pub async fn add_path_to_lib(
    settings: tauri::State<'_, Arc<RwLock<UserSettings>>>,
    path: String,
) -> Result<(), String> {
    let mut guard = settings.write().unwrap();
    if let Some(_) = guard.library_paths.iter().find(|p| **p == path) {
        return Err("Path already in lib".to_string());
    } else {
        guard.library_paths.push(path);
        guard.save().map_err(|err| err.to_string())?;
    }

    Ok(())
}
