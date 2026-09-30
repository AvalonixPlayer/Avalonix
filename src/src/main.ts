import { fillAlbumsLibrary } from "./scripts/albumsCreator";
import { initTabs } from "./scripts/tabsBinding";
import { fillTracksLibrary } from "./scripts/tracksCreator";

window.addEventListener("DOMContentLoaded", async () => {
  await init();
});

async function init() {
  await initTabs();
  await fillTracksLibrary();
  //await fillAlbumsLibrary();
}
