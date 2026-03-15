use std::{
    fs::File,
    io::BufReader,
    sync::{Arc, Mutex},
    thread,
    time::Duration,
};

use rodio::{
    Decoder, DeviceTrait, MixerDeviceSink, Player,
    cpal::{DeviceDescription, Host, traits::HostTrait},
};

use crate::logger;

struct Playback {
    mixer: MixerDeviceSink,
    last_device_description: DeviceDescription,
    player: Player,
    last_playing_track_path: Option<String>,
}

pub struct MediaPlayer {
    playback: Option<Playback>,
}

impl Playback {
    fn new() -> Result<Self, String> {
        match rodio::DeviceSinkBuilder::open_default_sink() {
            Ok(sink) => {
                let mixer = sink.mixer();
                let player = Player::connect_new(mixer);

                let host = Host::default();

                return Ok(Self {
                    mixer: sink,
                    last_device_description: host
                        .default_output_device()
                        .unwrap()
                        .description()
                        .unwrap(),
                    player,
                    last_playing_track_path: None,
                });
            }
            Err(_) => return Err("default device is None".to_string()),
        };
    }

    fn change_device(&mut self) {
        let new = Self::new();

        match new {
            Ok(new) => {
                let host = Host::default();
                let file_path = self.last_playing_track_path.as_ref().unwrap();
                new.play(file_path.clone());

                self.mixer = new.mixer;
                self.last_device_description =
                    host.default_output_device().unwrap().description().unwrap();
                self.player = new.player;
            }
            Err(_) => {}
        }
        /*
        let sink_handle =
            rodio::DeviceSinkBuilder::open_default_sink().expect("open default audio stream");

        let file_path = self.last_playing_track_path.as_ref().unwrap();

        self.player.stop();

        let player = Player::connect_new(&sink_handle.mixer());

        let host = Host::default();

        let file = BufReader::new(File::open(&file_path).unwrap());
        let source = Decoder::new(file).unwrap();

        player.append(source);

        self.mixer = sink_handle;
        self.last_device_description = host.default_output_device().unwrap().description().unwrap();
        self.player = player;
        */
    }

    fn play(&self, file_path: String) {
        let file = BufReader::new(File::open(file_path).unwrap());
        let source = Decoder::new(file).unwrap();
        self.player.stop();
        self.player.append(source);
    }

    fn stop(&self) {
        self.player.stop();
    }

    fn get_pos(&self) -> Duration {
        self.player.get_pos()
    }

    fn seek(&self, pos: Duration) {
        _ = self.player.try_seek(pos);
    }

    fn pause(&self) {
        self.player.pause();
    }

    fn cont(&self) {
        self.player.play();
    }
}

impl MediaPlayer {
    pub fn new() -> Result<Self, String> {
        let playback = Playback::new();
        match playback {
            Ok(pb) => Ok(Self { playback: Some(pb) }),
            Err(err) => {
                logger::error(&err);
                Err(err)
            }
        }
    }

    pub fn play(&mut self, file_path: String) {
        let playback = self.playback.as_mut().unwrap();

        playback.last_playing_track_path = Some(file_path.clone());
        playback.play(file_path);
    }

    pub fn stop(&self) {
        self.playback.as_ref().unwrap().stop();
    }

    pub fn get_pos(&self) -> Duration {
        self.playback.as_ref().unwrap().get_pos()
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

    pub fn update(clone: Arc<Mutex<MediaPlayer>>) {
        thread::spawn(move || {
            let host = Host::default();
            loop {
                let device = host.default_output_device();
                match device {
                    Some(_) => {}
                    None => logger::warn("can`t find default device"),
                }
                thread::sleep(Duration::new(1, 0));
                {
                    match device {
                        Some(device) => {
                            let mut guard = clone.try_lock().unwrap();

                            if guard.playback.as_ref().unwrap().last_device_description
                                != device.description().unwrap()
                            {
                                let playback: &mut Playback = guard.playback.as_mut().unwrap();
                                playback.change_device();
                            }
                        }
                        None => {}
                    }
                }
            }
        });
    }
}

#[test]
fn test_play() {
    let mp = MediaPlayer::new();
    match mp {
        Ok(player) => {
            let player = Arc::new(Mutex::new(player));

            let clone1 = player.clone();
            let clone2 = player.clone();

            let file_path =
        "D:\\music\\Three Days Grace [restored]\\2006 - One-X\\03. Animal I Have Become.flac"
            .to_string();

            MediaPlayer::update(clone2);

            clone1.lock().as_mut().unwrap().play(file_path);
        }
        Err(_) => {}
    }

    loop {}
}
// it took too much nerves
