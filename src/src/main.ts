import { listen } from "@tauri-apps/api/event";
import { fillAlbumsList } from "./scripts/albumsCreator.js";
import { fillPerformersList } from "./scripts/performerCreator.js";
import { initMainSectionControll } from "./scripts/tabsBinding.js";
import { fillTracksList } from "./scripts/tracksCreator.js";
import { fillPlayQueueList } from "./scripts/playQueue.js";

window.addEventListener("DOMContentLoaded", async () => {
  await init();
});

async function init() {
  console.log("start");
  initMainSectionControll();
  await fillTracksList();
  await fillAlbumsList();
  await fillPerformersList();
  await fillPlayQueueList();
  listen("queue-updated", async () => await fillPlayQueueList());
}
