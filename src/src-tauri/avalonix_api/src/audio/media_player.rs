use std::{
    fs::File,
    sync::{Arc, Mutex, mpsc::Sender},
    time::Duration,
};

use anyhow::{Context, Result};
use better_sms::mutex::MutexWork;
use rodio::{Decoder, DeviceSinkBuilder, MixerDeviceSink, Player};

use crate::{
    events::Event,
    media::track::Track,
};

pub struct MediaPlayer {
    _stream_handle: MixerDeviceSink,
    player: Player,
    cur_track: Option<Track>,
    events_sender: Arc<Mutex<Sender<Event>>>,
}

impl MediaPlayer {
    pub fn new(events_sender: &Arc<Mutex<Sender<Event>>>) -> Result<Self> {
        let _stream_handle = DeviceSinkBuilder::open_default_sink()?;
        let player = rodio::Player::connect_new(_stream_handle.mixer());

        Ok(Self {
            _stream_handle,
            player,
            cur_track: None,
            events_sender: events_sender.clone(),
        })
    }

    pub fn play_track(&mut self, track: &Track) -> Result<()> {
        let path = &track.path;
        let file = File::open(path)?;
        self.cur_track = Some(track.clone());
        self.stop();
        self.player.append(Decoder::try_from(file)?);
        self.play();
        self.seek(0)?;
        self.events_sender
            .lock_unw()
            .send(Event::CurTrackChanged)
            .unwrap();
        Ok(())
    }

    pub fn set_volume(&self, volume: f32) {
        self.player.set_volume(volume);
    }

    pub fn get_volume(&self) -> f32 {
        self.player.volume()
    }

    pub fn stop(&mut self) {
        self.player.stop();
        self.player.clear();
        self.events_sender
            .lock_unw()
            .send(Event::CurTrackChanged)
            .unwrap();
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

    pub fn empty(&self) -> bool {
        self.player.empty()
    }

    pub fn get_start_position(&self) -> u64 {
        0
    }

    pub fn get_end_pos(&self) -> u64 {
        self.cur_track
            .as_ref()
            .map(|track| {
                track
                    .end_time
                    .as_secs()
                    .saturating_sub(track.start_time.as_secs())
            })
            .unwrap_or(0)
    }

    pub fn get_cur_pos(&self) -> u64 {
        self.cur_track
            .as_ref()
            .map(|track| {
                self.player
                    .get_pos()
                    .as_secs()
                    .saturating_sub(track.start_time.as_secs())
            })
            .unwrap_or(0)
    }

    pub fn seek(&self, pos: u64) -> Result<()> {
        let track = self.cur_track.as_ref().context("track is empty")?;

        let target_secs = track.start_time.as_secs() + pos;
        self.player.try_seek(Duration::from_secs(target_secs))?;

        Ok(())
    }
}
