use std::{
    fs::File,
    sync::{Arc, Mutex, mpsc},
    thread,
    time::Duration,
};

use lofty::io::Length;
use rodio::{
    Decoder, DeviceTrait, MixerDeviceSink, Player, Source,
    cpal::{DeviceDescription, Host, traits::HostTrait},
};

use crate::{logger, media::metadata::Metadata, playable::track::Track, utils::get_argument_val};

struct Playback {
    stream_handle: MixerDeviceSink,
    last_device_description: DeviceDescription,
    player: Player,
    last_playing_track: Option<Arc<Mutex<Track>>>,
    total_time: Duration,
}

pub struct MediaPlayer {
    playback: Option<Playback>,
    pub sender: Option<mpsc::Sender<Metadata>>,
}

impl Playback {
    fn new() -> Result<Self, String> {
        let stream_handle = rodio::DeviceSinkBuilder::open_default_sink();
        match stream_handle {
            Ok(stream_handle) => {
                let player = Player::connect_new(&stream_handle.mixer());

                let host = Host::default();

                return Ok(Self {
                    stream_handle,
                    last_device_description: host
                        .default_output_device()
                        .unwrap()
                        .description()
                        .unwrap(),
                    player,
                    last_playing_track: None,
                    total_time: Duration::new(0, 0),
                });
            }
            Err(_) => return Err("default device is None".to_string()),
        };
    }

    fn change_device(&mut self) {
        let new = Self::new();

        match new {
            Ok(mut new) => {
                let host = Host::default();
                let last_playing_track = self.last_playing_track.clone().unwrap();

                new.play(&last_playing_track);

                self.stream_handle = new.stream_handle;
                self.last_device_description =
                    host.default_output_device().unwrap().description().unwrap();
                self.player = new.player;
            }
            Err(_) => {}
        }
    }

    fn play(&mut self, track_arc: &Arc<Mutex<Track>>) {
        match self.last_playing_track.clone() {
            Some(last_playing_track) => {
                let mut guard = last_playing_track.lock().unwrap();
                guard.metadata.track_cover_uri = None;
            }
            None => {}
        }
        let track_clone = track_arc.clone();

        let file = File::open(&track_clone.lock().unwrap().file_path).unwrap();
        let len = file.len().unwrap();

        let source = Decoder::builder()
            .with_data(file)
            .with_byte_len(len)
            .build()
            .unwrap();

        match source.total_duration() {
            Some(total_duration) => {
                self.total_time = total_duration;

                self.player.stop();
                self.player.append(source);
            }
            None => {
                logger::error(&format!(
                    "track with file path {} has incorrect durration",
                    track_clone.lock().unwrap().file_path
                ));
            }
        }
    }

    fn stop(&self) {
        self.player.stop();
    }

    fn get_pos(&self) -> Duration {
        self.player.get_pos()
    }

    fn get_len(&self) -> Duration {
        self.total_time.clone()
    }

    fn seek(&self, pos: Duration) {
        _ = self.player.try_seek(pos);
    }

    fn pause(&self) {
        self.player.pause();
    }

    fn is_paused(&self) -> bool {
        self.player.is_paused()
    }

    fn cont(&self) {
        self.player.play();
    }

    fn empty(&self) -> bool {
        self.player.empty()
    }
}

impl MediaPlayer {
    pub fn new() -> Result<Self, String> {
        let playback = Playback::new();
        match playback {
            Ok(pb) => Ok(Self {
                playback: Some(pb),
                sender: None,
            }),
            Err(err) => {
                logger::error(&err);
                Err(err)
            }
        }
    }

    pub fn play(&mut self, track_arc: &Arc<Mutex<Track>>) {
        let track_clone = track_arc.clone();
        let playback = self.playback.as_mut().unwrap();

        {
            let binding = track_clone.clone();
            let mut track_guard = binding.lock().unwrap();

            let fp = track_guard.file_path.clone();

            _ = track_guard.metadata.add_cover_to_metadata(&fp);

            let sender = self.sender.as_mut();
            match sender {
                Some(sender) => sender.send(track_guard.metadata.clone()).unwrap(),
                None => {}
            }
        }

        playback.last_playing_track = Some(track_clone);
        playback.play(track_arc);
    }

    pub fn stop(&self) {
        self.playback.as_ref().unwrap().stop();
    }

    pub fn get_pos(&self) -> Duration {
        self.playback.as_ref().unwrap().get_pos()
    }

    pub fn get_len(&self) -> Duration {
        self.playback.as_ref().unwrap().get_len()
    }

    pub fn seek(&self, pos: Duration) {
        self.playback.as_ref().unwrap().seek(pos);
    }

    pub fn pause(&self) {
        self.playback.as_ref().unwrap().pause();
    }

    pub fn cont(&self) {
        self.playback.as_ref().unwrap().cont();
    }

    pub fn is_paused(&self) -> bool {
        self.playback.as_ref().unwrap().is_paused()
    }

    pub fn empty(&self) -> bool {
        self.playback.as_ref().unwrap().empty()
    }

    pub fn update(player_arc: &Arc<Mutex<MediaPlayer>>) {
        let clone = player_arc.clone();

        thread::spawn(move || {
            let host = Host::default();
            loop {
                println!("Checking for device");
                let device = host.default_output_device();
                match device {
                    Some(_) => {}
                    None => logger::warn("can`t find default device"),
                }
                thread::sleep(Duration::new(1, 0));
                {
                    match device {
                        Some(device) => {
                            let mut player = clone.lock().unwrap();
                            if player.playback.as_mut().unwrap().last_device_description
                                != device.description().unwrap()
                            {
                                let playback = player.playback.as_mut().unwrap();
                                playback.change_device();
                            }
                            drop(player);
                        }
                        None => {}
                    }
                }
            }
        });
    }
}

#[test]
fn test_play_media_player() {
    use crate::{db::MusicDB, disk_manager};

    let track_path = get_argument_val(&"TRACK_PATH");
    let Some(_) = track_path else {
        return;
    };

    let mp = MediaPlayer::new();
    match mp {
        Ok(player) => {
            let player = Arc::new(Mutex::new(player));

            let file_path = track_path.unwrap();

            MediaPlayer::update(&player);

            let db = &MusicDB::open(&disk_manager::avalonix_special_folder_path()).unwrap();

            let all_tracks = db.get_all_tracks().unwrap();
            let tracks_hash = all_tracks.iter().collect();

            let track = Arc::new(Mutex::new(Track::new(&file_path, db, tracks_hash).unwrap()));

            player.clone().lock().as_mut().unwrap().play(&track);
        }
        Err(_) => {}
    }

    loop {}
}
// it took too much nerves
