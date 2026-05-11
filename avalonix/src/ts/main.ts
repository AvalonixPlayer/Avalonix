import { loadTracksFilerDatas } from "./mainFiller";
import { initTabs } from "./tabsInit";
import { initTrackPreview, updateTrackPreview } from "./trackPreview";
import { initPlaybackControll } from "./playbackControll";
import { loadAlbumsFilerDatas } from "./loadAlbumsFilterDatas";
import { loadPerformersFilerDatas } from "./loadPerformersFilterDatas";
import { initSettings } from "./settings";
import { initContextMenu } from "./contextMenu";
import { invoke } from "@tauri-apps/api/core";

init().then(() => {
  console.log("initilization end");
});

async function init() {
  initContextMenu();
  initTabs();
  await initSettings();
  await initPlaybackControll();
  await initTrackPreview();
  await getLibFromDB();
  await updateTrackPreview(null, null);
  await invoke("update_library");
  await getLibFromDB();
}

export async function getLibFromDB() {
  await loadTracksFilerDatas();
  await loadAlbumsFilerDatas();
  await loadPerformersFilerDatas();
}
