use std::{
    sync::{Arc, Mutex, MutexGuard},
    thread,
    time::Duration,
    usize,
};

use crate::{audio::media_player::MediaPlayer, logger, media::track::Track};

#[derive(ts_rs::TS)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\PlayQueue.ts")]
pub struct PlayQueue {
    pub tracks: Vec<Arc<Mutex<Track>>>,
    #[ts(skip)]
    media_player: Arc<Mutex<MediaPlayer>>,
    current_track_index: i32,
}

impl PlayQueue {
    pub fn new(media_player_arc: &Arc<Mutex<MediaPlayer>>) -> Self {
        PlayQueue {
            tracks: Vec::new(),
            media_player: media_player_arc.clone(),
            current_track_index: 0,
        }
    }

    pub fn clear(&mut self) {
        logger::debug("queue cleared");
        self.tracks.clear();
    }

    pub fn add_track(&mut self, track: Arc<Mutex<Track>>) {
        if self
            .tracks
            .iter()
            .any(|t| *t.lock().unwrap() == *track.lock().unwrap())
        {
            logger::warn(&format!(
                "track with id: {} also in play queue",
                track.lock().unwrap().id
            ));
            return;
        }
        self.tracks.push(track);
    }

    pub fn add_tracks(&mut self, tracks: Vec<Arc<Mutex<Track>>>) {
        for track in tracks {
            self.add_track(track);
        }
    }

    pub fn remove_track(&mut self, track: Arc<Mutex<Track>>) {
        if let Some(index) = self
            .tracks
            .iter()
            .position(|f| *f.lock().unwrap() == *track.lock().unwrap())
        {
            self.tracks.remove(index);

            if (index as i32) <= self.current_track_index {
                self.current_track_index -= 1;
            }

            logger::debug(&format!(
                "track with id: {} removed",
                track.lock().unwrap().id
            ));
            return;
        }
        logger::warn(&format!(
            "track with id: {} not found in queue",
            track.lock().unwrap().id
        ));
    }

    pub fn play(self_arc: &Arc<Mutex<Self>>, media_player_arc: &Arc<Mutex<MediaPlayer>>) {
        let self_clone = self_arc.clone();
        let media_player_clone = media_player_arc.clone();

        thread::spawn(move || {
            loop {
                let mut queue = self_clone.lock().unwrap();
                let mut player = media_player_clone.lock().unwrap();

                if !queue.tracks.is_empty() && player.empty() {
                    let next_index = (queue.current_track_index + 1) as usize;

                    let i = if next_index < queue.tracks.len() {
                        next_index
                    } else {
                        0
                    };

                    Self::play_by_index(&mut queue, &mut player, i);
                }

                drop(player);
                drop(queue);

                thread::sleep(Duration::from_millis(100));
            }
        });
    }

    fn play_by_index(
        self_guard: &mut MutexGuard<'_, Self>,
        media_player_guard: &mut MutexGuard<'_, MediaPlayer>,
        index: usize,
    ) {
        self_guard.current_track_index = index as i32;
        match self_guard.tracks.get(index) {
            Some(track) => {
                media_player_guard.play(&track);
            }
            None => {}
        }
    }

    pub fn pause_or_continue(&self) {
        let guard = self.media_player.lock().unwrap();

        match guard.is_paused() {
            true => {
                guard.cont();
            }
            false => {
                guard.pause();
            }
        }
    }

    pub fn next_track(self_arc: &Arc<Mutex<Self>>, media_player_arc: &Arc<Mutex<MediaPlayer>>) {
        let self_clone = self_arc.clone();
        let mp_clone = media_player_arc.clone();
        let mut self_guard = self_clone.lock().unwrap();
        let mut media_player_guard = mp_clone.lock().unwrap();

        if self_guard.tracks.len() == 0 {
            return;
        }

        let mut next_index = self_guard.current_track_index;

        if (self_guard.current_track_index + 1) as usize <= self_guard.tracks.len() - 1 {
            next_index += 1;
        } else {
            next_index = 0;
        }

        Self::play_by_index(
            &mut self_guard,
            &mut media_player_guard,
            next_index as usize,
        );
    }

    pub fn previous_track(self_arc: &Arc<Mutex<Self>>, media_player_arc: &Arc<Mutex<MediaPlayer>>) {
        let self_clone = self_arc.clone();
        let mp_clone = media_player_arc.clone();
        let mut self_guard = self_clone.lock().unwrap();
        let mut media_player_guard = mp_clone.lock().unwrap();

        if self_guard.tracks.len() == 0 {
            return;
        }

        let mut previous_index = self_guard.current_track_index;

        if self_guard.current_track_index - 1 >= 0 {
            previous_index -= 1;
        } else {
            previous_index = (self_guard.tracks.len() - 1) as i32;
        }

        Self::play_by_index(
            &mut self_guard,
            &mut media_player_guard,
            previous_index as usize,
        );
    }
}

#[test]
fn test_play_queue() {
    use crate::db::MusicDB;
    use crate::disk_manager;
    use crate::playboxes::playboxes::TracksContainer;
    let hash_path = disk_manager::avalonix_special_folder_path();
    let db = MusicDB::open(&hash_path).unwrap();
    let cont = TracksContainer::new(&db);

    let mp = MediaPlayer::new().unwrap();

    let media_player = Arc::new(Mutex::new(mp));
    MediaPlayer::update(&media_player);

    let queue = Arc::new(Mutex::new(PlayQueue::new(&media_player)));
    {
        let mut queue_guard = queue.lock().unwrap();

        queue_guard.add_tracks(cont.all_tracks.clone());
    }

    PlayQueue::play(&queue, &media_player);

    loop {}
}
