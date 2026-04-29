use std::{
    num::NonZero,
    sync::{Arc, Mutex},
    thread::{self, sleep},
    time::Duration,
};

use anyhow::Ok;
use rodio::{
    Decoder, DeviceTrait, MixerDeviceSink, Player, Source,
    cpal::{BufferSize, DeviceDescription, default_host, traits::HostTrait},
    source,
};

use crate::{logger, media::track::Track, mutex_work::CreateArcMutex};

pub struct MediaPlayer {
    _sink: MixerDeviceSink,
    total_duration: Duration,
    player: Player,
    last_device_description: DeviceDescription,
}

impl MediaPlayer {
    pub fn new() -> anyhow::Result<Arc<Mutex<Self>>> {
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
            total_duration: Duration::new(0, 0),
            player,
            last_device_description: device_description,
        }
        .create_arc_mutex();

        MediaPlayer::check_status(&self_arc);

        Ok(self_arc)
    }

    pub fn is_empty(&self) -> bool {
        self.player.empty()
    }

    pub fn start_audio(&mut self, track: &Track) -> anyhow::Result<()> {
        let data = track.get_data()?;
        let len = data.get_ref().len() as u64;

        let mut source = Decoder::builder()
            .with_data(data)
            .with_byte_len(len)
            .build()?;

        let _ = source.try_seek(track.metadata.start_pos)?;
        let source = source.take_duration(track.metadata.end_pos);

        self.total_duration = source.total_duration().unwrap();

        self.player.clear();
        self.player.append(source);
        self.player.play();

        logger::debug("audio started");
        Ok(())
    }

    pub fn pause(&mut self) {
        self.player.pause();
        logger::debug("audio paused");
    }

    pub fn play(&mut self) {
        self.player.play();
        logger::debug("audio played");
    }

    pub fn stop_audio(&mut self) {
        self.player.stop();
        self.player.clear();
        logger::debug("audio stoped");
    }

    pub fn seek(&mut self, pos: Duration) -> anyhow::Result<()> {
        self.player.try_seek(pos)?;
        logger::debug(format!("player seeked to: {}", pos.as_secs()));
        Ok(())
    }

    pub fn get_len(&mut self) -> Duration {
        self.total_duration
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

        let dur = Duration::from_secs(1);
        thread::spawn(move || {
            loop {
                sleep(dur);
                let mut self_guard = self_clone.lock().unwrap();
                _ = self_guard
                    .device_changed_check()
                    .map_err(|err| logger::error(err));
            }
        });
    }
}

#[test]
fn test_media_player() -> anyhow::Result<()> {
    use crate::{disk::db::DB, utils::get_argument_val};
    use std::path::PathBuf;
    let media_player = MediaPlayer::new()?;
    let track_path = get_argument_val("TRACK_PATH").unwrap();

    let db: DB = DB::open()?;

    let path = PathBuf::from(&track_path);

    let binding = Track::create_tracks_list_from_file(&path, &db)?;

    let track = binding.last().clone().unwrap();

    let sleep = || thread::sleep(Duration::from_secs(1));
    {
        let mut guard = media_player.lock().unwrap();
        guard.start_audio(&track)?; // for cue need fix
    }
    sleep();
    {
        let mut guard = media_player.lock().unwrap();
        guard.pause();
    }
    sleep();
    {
        let mut guard = media_player.lock().unwrap();
        guard.play();
    }
    sleep();
    {
        let mut guard = media_player.lock().unwrap();
        let len = guard.total_duration;
        guard.seek(len / 2)?;
    }
    sleep();
    {
        let mut guard = media_player.lock().unwrap();
        guard.stop_audio();
    }
    sleep();

    Ok(())
}
