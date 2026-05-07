#[cfg(test)]
mod tests {
    use std::{sync::mpsc, thread, time::Duration};

    use anyhow::Ok;

    use crate::{
        disk::{
            db::{self, DB},
            disk_manager,
            settings::Settings,
        },
        media::{media_player::MediaPlayer, track::Track},
        utils::get_argument_val,
    };

    #[test]
    fn test_settings() -> anyhow::Result<()> {
        let settings = Settings::open()?;
        Ok(())
    }

    #[test]

    fn test_db() -> anyhow::Result<()> {
        let settings = &Settings::open()?;
        let db = DB::open()?;

        let mut guard = db.lock().unwrap();
        guard.update_library(settings)?;
        Ok(())
    }

    #[test]
    fn test_disk_manager() -> anyhow::Result<()> {
        let settings = &Settings::open()?;
        disk_manager::avalonix_folder_path();
        disk_manager::db_path();
        disk_manager::get_tracks_files_paths(settings);
        disk_manager::lib_db_path();
        disk_manager::settings_path();
        Ok(())
    }

    #[test]
    fn test_media_player() -> anyhow::Result<()> {
        let db = &DB::open()?;
        let db_guard = db.lock().unwrap();
        if let Some(first_track) = db_guard.get_tracks_hash()?.get(0) {
            let (sender, reciver) = mpsc::channel();
            let media_player = MediaPlayer::new(&sender)?;
            let mut media_player_guard = media_player.lock().unwrap();
            media_player_guard.start_audio(first_track)?;
        }
        thread::sleep(Duration::from_secs(10));

        Ok(())
    }
}
