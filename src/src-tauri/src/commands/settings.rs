use std::{
    fs,
    ops::DerefMut,
    sync::{Arc, RwLock},
    thread::Thread,
};

use avalonix_api::{
    disk::user::{settings::UserSettings, theme::Theme},
    logger::error,
};
use base64::{engine::general_purpose, Engine};

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

#[tauri::command]
pub async fn get_theme(
    settings: tauri::State<'_, Arc<RwLock<UserSettings>>>,
) -> Result<Theme, String> {
    let theme = settings.read().unwrap().themes[0].clone();
    Ok(theme)
}

#[tauri::command]
pub async fn save_settings(
    settings: tauri::State<'_, Arc<RwLock<UserSettings>>>,
) -> Result<(), String> {
    settings
        .write()
        .unwrap()
        .save()
        .map_err(|err| err.to_string())?;
    Ok(())
}

#[tauri::command]
pub async fn set_theme(
    theme: Theme,
    settings: tauri::State<'_, Arc<RwLock<UserSettings>>>,
) -> Result<(), String> {
    settings.write().unwrap().themes[0] = theme;
    Ok(())
}

#[tauri::command]
pub async fn get_bg_gif_uri(
    settings: tauri::State<'_, Arc<RwLock<UserSettings>>>,
) -> Result<String, String> {
    if let Some(path) = settings.read().unwrap().themes[0]
        .path_to_background_image
        .clone()
    {
        let image_bytes = fs::read(path).map_err(|err| err.to_string())?;
        let base64_encoded = general_purpose::STANDARD.encode(&image_bytes);

        let data_uri = format!("data:image/gif;base64,{}", base64_encoded);
        return Ok(data_uri);
    }
    Err("No path to gif".to_string())
}
