import { invoke } from "@tauri-apps/api/core";
import { initTabs } from "./scripts/tabsBinding";
import { fillTracksLibrary } from "./scripts/tracksCreator";

window.addEventListener("DOMContentLoaded", async () => {
  await init();
});

async function init() {
  await initTabs();
  await fillTracksLibrary();
}
