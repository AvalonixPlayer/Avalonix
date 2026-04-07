import { Metadata } from "./bindings/Metadata.js";
import { UpdateTrackQueueUI } from "./playQueue.js";
import "./tabSwitcher.js";
import "./playbackBtns.ts";
import { UpdateTrackUI } from "./metadataUpdate.ts";
import { initLib } from "./playboxes.ts";

(async () => {
  await initLib();
  await tracksFillerInit();
  await UpdateTrackQueueUI();
  await UpdateTrackUI(null);
  await bindPlayback();
})();

setInterval(() => {
  setBG();
}, 10);

import { listen } from "@tauri-apps/api/event";
import { bindPlayback } from "./playbackBtns.ts";
import { tracksFillerInit } from "./tracksFiller.ts";

listen<Metadata>("playing-track-updated", async (event) => {
  await UpdateTrackUI(event.payload);
});

listen("play-queue-action-compleated", async (_) => {
  await UpdateTrackQueueUI();
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
