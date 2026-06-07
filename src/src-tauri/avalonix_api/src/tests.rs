#[cfg(test)]
mod tests {
    use std::{
        env,
        sync::{Arc, Mutex},
        thread::{self, sleep},
        time::Duration,
    };

    use anyhow::Result;

    use crate::{
        audio::media_player::MediaPlayer,
        disk::{db::DB, user::settings::UserSettings},
        logger::{debug, error},
        media::{
            album::Album,
            media_trait::MediaType::{self},
            performer::Performer,
            play_queue,
            play_queue::PlayQueue,
            track::Track,
        },
    };

    #[test]
    fn test_db() -> Result<()> {
        let args: Vec<String> = env::args().collect();
        let db = DB::open()?;
        let every_tracks_in_db: &[Track] = &db.get_every_track()?[0..];

        if let Ok(media_path) = std::env::var("ADD_MEDIA") {
            let tracks = Track::get_tracks_by_path(&media_path, every_tracks_in_db, &db)?;
            let albums = Album::create_albums(&db, &db.get_every_track()?[0..])?;
            let performers = Performer::create_performers(&db, &db.get_every_track()?[0..])?;
        }

        db.update();
        let tracks = db.get_every_track()?;
        let albums = db.get_every_album()?;
        let performers = db.get_every_performer()?;
        debug(format!("tracks: {}", tracks.len()));
        debug(format!("albums: {}", albums.len()));
        debug(format!("performers: {}", performers.len()));

        for i in tracks {
            debug(i);
        }

        for i in albums {
            debug(i);
        }

        for i in performers {
            debug(i);
        }
        Ok(())
    }

    #[test]
    fn test_media_player() -> Result<()> {
        let player = MediaPlayer::new()?;
        if let Ok(media_path) = std::env::var("TRACK_FILE_PATH") {
            player.play_audio_file(media_path);
            println!("{}", player.get_volume());
            player.set_volume(0.5);
            thread::sleep(Duration::new(1, 0));
            player.change_pause_state();
            thread::sleep(Duration::new(1, 0));
            player.change_pause_state();
            thread::sleep(Duration::new(1000, 0));
        }

        Ok(())
    }

    #[test]
    fn test_settings() -> Result<()> {
        let mut settings = UserSettings::open()?;
        if let Ok(media_path) = std::env::var("LIBRARY_PATH") {
            settings.library_paths.push(media_path);
            settings.save()?;
        }
        Ok(())
    }

    #[test]
    fn test_play_queue() -> Result<()> {
        let mut queue = PlayQueue::new();
        let player = Arc::new(Mutex::new(MediaPlayer::new()?));

        let db = Arc::new(Mutex::new(DB::open()?));
        let every_tracks_in_db = &db.lock().unwrap().get_every_track()?;

        queue.add_track(every_tracks_in_db[0].uuid.clone());

        let queue_arc = Arc::new(Mutex::new(queue));
        PlayQueue::play(&queue_arc, &player, &db);
        loop {
            sleep(Duration::new(5, 0));
            queue_arc.lock().unwrap().next(&player);
            sleep(Duration::new(5, 0));
            queue_arc.lock().unwrap().back(&player);
        }

        Ok(())
    }
}
