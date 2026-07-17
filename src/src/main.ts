import { listen } from "@tauri-apps/api/event";
import { fillAlbumsList } from "./scripts/albumsCreator.js";
import { fillPerformersList } from "./scripts/performerCreator.js";
import { initMainSectionControll } from "./scripts/tabsBinding.js";
import { fillTracksList } from "./scripts/tracksCreator.js";
import { fillPlayQueueList } from "./scripts/playQueue.js";
import { invoke } from "@tauri-apps/api/core";
import { initPlayback } from "./scripts/bindPlayblack.js";
import { initPlaybackControll } from "./scripts/playbackControll.js";
import { initResizeControll } from "./scripts/resizeControll.js";
import { regCustomElements } from "./scripts/customElements.js";
import { initSettings } from "./scripts/settings.js";

window.addEventListener("DOMContentLoaded", async () => {
  await init();
});

async function init() {
  regCustomElements();
  await initSettings();
  console.log("start");
  initMainSectionControll();
  initResizeControll();
  await initPlaybackControll();
  await initPlayback();
  await loadLib();
  await invoke("update_library");
  console.log("library_updated");
  await loadLib();
  listen("queue-updated", async () => await fillPlayQueueList());
}

async function loadLib() {
  await fillTracksList();
  await fillAlbumsList();
  await fillPerformersList();
  await fillPlayQueueList();
}
