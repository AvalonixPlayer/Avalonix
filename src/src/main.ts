import { fillAlbumsList } from "./scripts/albumsCreator.js";
import { fillPerformersList } from "./scripts/performerCreator.js";
import { initMainSectionControll } from "./scripts/tabsBinding.js";
import { fillTracksList } from "./scripts/tracksCreator.js";

window.addEventListener("DOMContentLoaded", () => {
  init();
});

async function init() {
  initMainSectionControll();
  await fillTracksList();
  await fillAlbumsList();
  await fillPerformersList();
}
