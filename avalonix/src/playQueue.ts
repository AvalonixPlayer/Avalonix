import { invoke } from "@tauri-apps/api/core";
import { Track } from "./bindings/Track";

export async function addTrackToQueue(track: Track) {
  await invoke('add_track_to_queue', {track: track});
}