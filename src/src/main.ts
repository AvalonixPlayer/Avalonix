import { fillAlbumsLibrary } from "./scripts/albumsCreator";
import { updateLibrary } from "./scripts/library";
import { initPlayqueueControll } from "./scripts/playQueue";
import { resize } from "./scripts/resizeControll";
import { initSliders } from "./scripts/slider";
import { initTabs } from "./scripts/tabsBinding";
import { fillTracksLibrary } from "./scripts/tracksCreator";

window.addEventListener("DOMContentLoaded", async () => {
  await init();
});

async function init() {
  await updateLibrary();
  await initTabs();
  await resize();
  await initSliders();

  await initPlayqueueControll();
}
