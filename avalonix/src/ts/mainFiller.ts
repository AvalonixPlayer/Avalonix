import { invoke } from "@tauri-apps/api/core";
import { TrackFilterMetadata } from "../bindings/TrackFilterMetadata";

let tracksFilerDatas: Array<TrackFilterMetadata>;

export async function loadTracksFilerDatas() {
  await invoke<Array<TrackFilterMetadata>>("get_tracks_filter_datas")
    .then((data) => {
      tracksFilerDatas = data;
      console.log(data);
    })
    .catch((e) => console.error(e));
}

export async function fillTracksList() {}
