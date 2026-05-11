use std::{
    fs::File,
    io::BufReader,
    num::NonZero,
    sync::{Arc, Mutex, mpsc::Sender},
    thread::{self, sleep},
    time::Duration,
};

use anyhow::Ok;
use rodio::{
    Decoder, DeviceTrait, MixerDeviceSink, Player, Source,
    cpal::{BufferSize, DeviceDescription, default_host, traits::HostTrait},
};

use crate::{events::Event, logger, media::track::Track, mutex_work::CreateArcMutex};

/// Audio player structure
pub struct MediaPlayer {
    _sink: MixerDeviceSink,
    start_position: Duration,
    end_position: Duration,
    player: Player,
    last_device_description: DeviceDescription,
    event_sender: Sender<Event>,
}

impl MediaPlayer {
    /// Creates a new audio player, running a background check
    pub fn new(event_sender: &Sender<Event>) -> anyhow::Result<Arc<Mutex<Self>>> {
        let stream_handle = rodio::DeviceSinkBuilder::from_default_device()?
            .with_buffer_size(BufferSize::Fixed(256))
            .with_sample_rate(NonZero::new(48_000).unwrap())
            .with_sample_format(rodio::cpal::SampleFormat::F64)
            .open_sink_or_fallback()?;

        let device_description = default_host()
            .default_output_device()
            .unwrap()
            .description()?;

        let player = Player::connect_new(stream_handle.mixer());
        let self_arc = Self {
            _sink: stream_handle,
            start_position: Duration::new(0, 0),
            end_position: Duration::new(0, 0),
            player,
            last_device_description: device_description,
            event_sender: event_sender.clone(),
        }
        .create_arc_mutex();

        MediaPlayer::check_status(&self_arc);

        Ok(self_arc)
    }

    pub fn is_empty(&self) -> bool {
        self.player.empty()
    }

    /// The track starts playing
    pub fn start_audio(&mut self, track: &Track) -> anyhow::Result<()> {
        //let data = track.get_data()?;
        let file = File::open(&track.metadata.file_path).unwrap();
        let len = file.metadata().unwrap().len();
        let data = BufReader::new(file);

        let source = Decoder::builder()
            .with_data(data)
            .with_byte_len(len)
            .build()?;

        self.start_position = track.metadata.start_pos;
        self.end_position = track.metadata.end_pos;

        self.player.clear();
        self.player.append(source);
        self.seek(Duration::from_secs(0))?;

        self.play();

        logger::debug("audio started");

        self.event_sender.send(Event::UpdatePlayingTrack).unwrap();
        Ok(())
    }

    fn pause(&mut self) {
        self.player.pause();
        logger::debug("audio paused");
    }

    fn play(&mut self) {
        self.player.play();
        logger::debug("audio played");
    }

    /// Changes the pause state
    pub fn pause_or_continue(&mut self) -> bool {
        match self.player.is_paused() {
            true => {
                self.play();
                true
            }
            false => {
                self.pause();
                false
            }
        }
    }

    pub fn is_paused(&self) -> bool {
        self.player.is_paused()
    }

    pub fn stop_audio(&mut self) {
        self.player.stop();
        self.player.clear();
    }

    pub fn seek(&mut self, pos: Duration) -> anyhow::Result<()> {
        self.player.try_seek(pos + self.start_position)?;
        logger::debug(format!("player seeked to: {}", pos.as_secs()));
        Ok(())
    }

    /// Returns the duration of the track playing in the media player
    pub fn get_len(&mut self) -> Duration {
        self.end_position - self.start_position
    }

    /// Returns the track time now
    pub fn get_pos(&mut self) -> Duration {
        let current_pos = self.player.get_pos();
        current_pos.saturating_sub(self.start_position)
    }

    /// Changes player volume
    ///
    /// 1.0 is a base volume (100%)
    pub fn set_volume(&self, volume: f32) {
        self.player.set_volume(volume);
    }

    /// Returns current volume value
    pub fn get_volume(&self) -> f32 {
        self.player.volume()
    }

    fn device_changed_check(&mut self) -> anyhow::Result<()> {
        let device_description = default_host()
            .default_output_device()
            .unwrap()
            .description()?;

        if device_description != self.last_device_description {
            self.change_device(device_description);
            logger::warn("device changed");
        }

        Ok(())
    }

    fn change_device(&mut self, description: DeviceDescription) {
        self.last_device_description = description;
    }
}

impl MediaPlayer {
    fn check_status(self_arc: &Arc<Mutex<Self>>) {
        let self_clone = self_arc.clone();

        let dur = Duration::from_millis(100);
        thread::spawn(move || {
            loop {
                sleep(dur);
                let mut self_guard = self_clone.lock().unwrap();
                if self_guard.get_pos()
                    > self_guard
                        .end_position
                        .saturating_sub(self_guard.start_position)
                {
                    self_guard.stop_audio();
                }
                _ = self_guard
                    .device_changed_check()
                    .map_err(|err| logger::error(err));
            }
        });
    }
}
