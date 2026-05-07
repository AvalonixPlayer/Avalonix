import { invoke } from "@tauri-apps/api/core";
import { loadTracksFilerDatas } from "./mainFiller";
import { tabsBtnsConnectToFunctions } from "./tabsBtnsConnectToFunctions";
import { initTrackPreview, updateTrackPreview } from "./trackPreview";
import { initPlaybackControll } from "./playbackControll";
import { loadAlbumsFilerDatas } from "./loadAlbumsFilterDatas";
import { loadPerformersFilerDatas } from "./loadPerformersFilterDatas";
import { initSettings } from "./settings";

init().then(() => {
  console.log("initilization end");
});

async function init() {
  tabsBtnsConnectToFunctions();
  await initSettings();
  await initPlaybackControll();
  await initTrackPreview();
  await getLibFromDB();
  await updateTrackPreview(null, null);
  //await invoke("update_library");
  await getLibFromDB();
}

export async function getLibFromDB() {
  await loadTracksFilerDatas();
  await loadAlbumsFilerDatas();
  await loadPerformersFilerDatas();
}
