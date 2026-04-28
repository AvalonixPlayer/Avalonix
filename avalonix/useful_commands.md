cargo test --lib -- --nocapture
cargo test export_bindings
$env:TRACK_PATH="D:\music\Three Days Grace [restored]\2006 - One-X\03. Animal I Have Become.flac"; cargo test test_play_media_player - example

ts_rs
$env:TS_RS_EXPORT_DIR="full path"; cargo test export_bindings
