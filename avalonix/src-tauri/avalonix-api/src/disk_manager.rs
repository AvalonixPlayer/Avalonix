use crate::utils::{OS, current_os};
use std::{collections::HashSet, env::var, path::PathBuf};

use glob::glob;

const MUSIC_FILES_EXTS: [&str; 5] = ["*.mp3", "*.flac", "*.m4a", "*.wav", "*.waw"];

fn user_path() -> PathBuf {
    match current_os() {
        OS::Windows => PathBuf::from(var("USERPROFILE").expect("sth wrong with user path")),
        OS::Linux | OS::MacOs => PathBuf::from(var("HOME").expect("sth wrong with user path")),
        _ => panic!("unknown os"),
    }
}

pub fn avalonix_special_folder_path() -> String {
    let path = user_path().join(".avalonix");
    path.to_str().unwrap().to_string()
}

pub fn get_all_tracks_paths() -> Vec<String> {
    use crate::logger;

    let mut dirs_paths: Vec<String> = Vec::new();

    dirs_paths.push("D:\\music".to_string());

    let mut hash_set: HashSet<String> = HashSet::new();

    for t in MUSIC_FILES_EXTS {
        for dir in &dirs_paths {
            let expr = format!("{}/**/{}", dir, t);
            for entry in glob(&expr).unwrap() {
                let str = entry.unwrap().to_str().unwrap().to_string();
                hash_set.insert(str.clone());
                logger::debug(&format!("{}", str));
            }
        }
    }

    hash_set.into_iter().collect()
}

#[test]
fn test_avalonix_folder_path() {
    println!("{:#?}", avalonix_special_folder_path());
}

#[test]
fn test_get_all_tracks_paths() {
    get_all_tracks_paths();
}
