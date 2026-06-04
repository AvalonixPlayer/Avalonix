use std::fs::File;

use anyhow::Result;
use rodio::{Decoder, DeviceSinkBuilder, MixerDeviceSink, Player};

pub struct MediaPlayer {
    _stream_handle: MixerDeviceSink,
    player: Player,
}

impl MediaPlayer {
    pub fn new() -> Result<Self> {
        let _stream_handle = DeviceSinkBuilder::open_default_sink()?;
        let player = rodio::Player::connect_new(_stream_handle.mixer());

        Ok(Self {
            _stream_handle,
            player,
        })
    }

    pub fn play_audio_file<P: AsRef<str>>(&self, path: P) -> Result<()> {
        let file = File::open(path.as_ref())?;
        self.stop();
        self.player.append(Decoder::try_from(file)?);
        self.play();
        Ok(())
    }

    pub fn set_volume(&self, volume: f32) {
        self.player.set_volume(volume);
    }

    pub fn get_volume(&self) -> f32 {
        self.player.volume()
    }

    pub fn stop(&self) {
        self.player.stop();
    }

    fn play(&self) {
        self.player.play();
    }

    fn pause(&self) {
        self.player.pause();
    }

    pub fn is_paused(&self) -> bool {
        self.player.is_paused()
    }

    pub fn change_pause_state(&self) {
        if self.is_paused() {
            self.play();
        } else {
            self.pause();
        }
    }
}
