use std::{
    sync::{Arc, Mutex, MutexGuard, mpsc},
    thread,
    time::{Duration, Instant},
    usize,
};

use crate::{audio::media_player::MediaPlayer, logger, media::track::Track};

pub enum PlayQueueAction {
    Clear,
    AddTrack(Arc<Mutex<Track>>),
    AddTracks(Vec<Arc<Mutex<Track>>>),
    RemoveTrack(String),
    PauseOrContinue,
    Next,
    Previous,
}

#[derive(ts_rs::TS)]
#[ts(export, export_to = "..\\..\\..\\src\\bindings\\PlayQueue.ts")]
pub struct PlayQueue {
    pub tracks: Vec<Arc<Mutex<Track>>>,
    #[ts(skip)]
    media_player: Arc<Mutex<MediaPlayer>>,
    #[ts(skip)]
    action_reciver: Arc<Mutex<mpsc::Receiver<PlayQueueAction>>>,
    #[ts(skip)]
    action_compleated_sender: Arc<Mutex<mpsc::Sender<()>>>,
    current_track_index: i32,
}

impl PlayQueue {
    pub fn new(
        media_player_arc: &Arc<Mutex<MediaPlayer>>,
        action_reciver_arc: &Arc<Mutex<mpsc::Receiver<PlayQueueAction>>>,
        action_compleated_sender_arc: &Arc<Mutex<mpsc::Sender<()>>>,
    ) -> Self {
        PlayQueue {
            tracks: Vec::new(),
            media_player: media_player_arc.clone(),
            action_reciver: action_reciver_arc.clone(),
            action_compleated_sender: action_compleated_sender_arc.clone(),
            current_track_index: 0,
        }
    }

    pub fn play(self_arc: &Arc<Mutex<Self>>) {
        let self_clone = self_arc.clone();

        let media_player_clone: Arc<Mutex<MediaPlayer>>;

        {
            let guard = self_clone.lock().unwrap();
            media_player_clone = guard.media_player.clone();
        }

        thread::spawn(move || {
            loop {
                {
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
                }

                {
                    let reciver: Arc<Mutex<mpsc::Receiver<PlayQueueAction>>>;
                    {
                        let queue_clone = self_clone.clone();
                        let g = queue_clone.lock().unwrap();
                        reciver = g.action_reciver.clone();
                    }

                    let reciver_guard = reciver.lock().unwrap();
                    let recived_action = reciver_guard.try_recv();

                    let action_compleated_sender;
                    {
                        let queue_clone = self_clone.clone();
                        let g = queue_clone.lock().unwrap();
                        action_compleated_sender = g.action_compleated_sender.clone();
                    }
                    let action_compleated_sender_guard = action_compleated_sender.lock().unwrap();

                    match recived_action {
                        Ok(recived_action) => match recived_action {
                            PlayQueueAction::Clear => {
                                logger::debug("queue cleared");
                                let mut queue_guard = self_clone.lock().unwrap();
                                queue_guard.tracks.clear();
                                action_compleated_sender_guard.send(()).unwrap();
                            }
                            PlayQueueAction::AddTrack(track) => {
                                Self::add_track(&track, &self_clone);
                                action_compleated_sender_guard.send(()).unwrap();
                            }
                            PlayQueueAction::AddTracks(tracks) => {
                                for track in tracks {
                                    Self::add_track(&track, &self_clone);
                                }
                                action_compleated_sender_guard.send(()).unwrap();
                            }
                            PlayQueueAction::RemoveTrack(track_path) => {
                                Self::remove_track(&track_path, &self_clone);
                                action_compleated_sender_guard.send(()).unwrap();
                            }
                            PlayQueueAction::PauseOrContinue => {
                                let self_guard = self_clone.lock().unwrap();
                                let media_player_guard = self_guard.media_player.lock().unwrap();

                                match media_player_guard.is_paused() {
                                    true => {
                                        media_player_guard.cont();
                                    }
                                    false => {
                                        media_player_guard.pause();
                                    }
                                }
                            }
                            PlayQueueAction::Next => {
                                Self::next_track(&self_clone, &media_player_clone);
                            }
                            PlayQueueAction::Previous => {
                                Self::previous_track(&self_clone, &media_player_clone);
                            }
                        },
                        _ => {}
                    }
                }

                thread::sleep(Duration::from_millis(100));
            }
        });
    }

    fn add_track(track_arc: &Arc<Mutex<Track>>, queue_arc: &Arc<Mutex<PlayQueue>>) {
        let track = track_arc.clone();
        let queue = queue_arc.clone();
        let mut queue_guard = queue.lock().unwrap();

        let file_path = track.lock().unwrap().file_path.clone();
        if queue_guard
            .tracks
            .iter()
            .any(|t| t.lock().unwrap().file_path == file_path)
        {
            {
                let guard = track.lock().unwrap();
                logger::debug(&format!("track with id: {} also in queue", guard.id));
            }
            return;
        }
        {
            let guard = track.lock().unwrap();
            logger::debug(&format!("track with id: {} added to queue", guard.id));
        }
        queue_guard.tracks.push(track);
    }

    fn remove_track(track_path: &String, self_arc: &Arc<Mutex<Self>>) {
        let self_clone = self_arc.clone();
        let mut self_guard = self_clone.lock().unwrap();

        if let Some(index) = self_guard
            .tracks
            .iter()
            .position(|f| f.lock().unwrap().file_path == *track_path)
        {
            self_guard.tracks.remove(index);
            if (index as i32) <= self_guard.current_track_index {
                self_guard.current_track_index -= 1;
            }
        }
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

    fn next_track(self_arc: &Arc<Mutex<Self>>, media_player_arc: &Arc<Mutex<MediaPlayer>>) {
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

    fn previous_track(self_arc: &Arc<Mutex<Self>>, media_player_arc: &Arc<Mutex<MediaPlayer>>) {
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
    /*
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
     */
}
