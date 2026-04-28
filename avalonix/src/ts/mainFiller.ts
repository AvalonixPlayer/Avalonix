import { invoke } from "@tauri-apps/api/core";

let tracksIds: Array<Number>;

export async function loadTracksIds() {
  await invoke<Array<Number>>("get_tracks_ids")
    .then((ids) => {
      tracksIds = ids;
    })
    .catch((e) => console.error(e));
}

export async function fillTracksList() {}
