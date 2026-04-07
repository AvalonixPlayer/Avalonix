# Avalonix

Simple, user-friendly, optimizated, cross-platform audio player

# Technologies used

- [tauri](https://v2.tauri.app/) for GUI
- [lofty-rs](https://github.com/Serial-ATA/lofty-rs) for metadata processing
- [rodio](https://github.com/RustAudio/rodio?tab=Apache-2.0-1-ov-file) for audio playing
- [sled](https://github.com/spacejam/sled) for db working

# Feutures

- Audio playing
- Metadata parsing and overwriting (the re-recording hasn't been done yet, but it definitely will be)
- Tray

# Roadmap

- [x] audio playing
- [x] db
- [x] metdata hashing
- [ ] connect api to tauri
- [ ] customization
- [ ] equalizer

# Story

Previously, we developed Avalonix together with [**Ruzen**](https://github.com/ruzen42) on [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia), but now we decided to rebuild it using [rust](https://v2.tauri.app/)+[tauri](https://v2.tauri.app/): the initial version had many errors.
