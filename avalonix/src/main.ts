import { Metadata } from "./bindings/Metadata.js";
import { getAllAlbums, getAllArtists, getAllTracks } from "./playboxesFiller";
import { UpdateTrackQueueUI } from "./playQueue.js";
import "./tabSwitcher.js";
import "./playbackBtns.ts";
import { UpdateTrackUI } from "./metadataUpdate.ts";

(async () => {
  await getAllTracks();
  await getAllAlbums();
  await getAllArtists();
  await UpdateTrackQueueUI();
  await UpdateTrackUI(null);
  await bindPlayback();
})();

setInterval(() => {
  setBG();
}, 10);

import { listen } from "@tauri-apps/api/event";
import { bindPlayback } from "./playbackBtns.ts";

listen<Metadata>("playing-track-updated", async (event) => {
  await UpdateTrackUI(event.payload);
});

async function setBG() {
  let x = window.screenX / 2 + window.innerWidth / 2;
  let y = window.screenY / 2 + window.innerHeight / 2;
  const col1 = getComputedStyle(document.documentElement).getPropertyValue(
    "--background-gradient1",
  );
  const col2 = getComputedStyle(document.documentElement).getPropertyValue(
    "--background-gradient2",
  );
  document.body.style.background = `radial-gradient(circle 350px at ${x}px ${y}px, ${col1}, ${col2} )`;
}
