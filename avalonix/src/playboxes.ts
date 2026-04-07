import { invoke } from "@tauri-apps/api/core";
import { FilterData } from "./bindings/FilterData";

export let allTracksId: Array<Array<number>>;
export let allTracksFilterData: Array<FilterData>;

export async function initLib() {
  await getAllTracksId();
  await getAllTracksFilterData();
}

async function getAllTracksId() {
  allTracksId = await invoke<Array<Array<number>>>("get_all_tracks_id");
}

async function getAllTracksFilterData() {
  allTracksFilterData = await invoke<Array<FilterData>>(
    "get_all_tracks_filter_data",
  );
  console.log(allTracksFilterData);
}
