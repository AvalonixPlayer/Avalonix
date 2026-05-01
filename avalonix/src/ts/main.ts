import { invoke } from "@tauri-apps/api/core";
import { loadTracksFilerDatas } from "./mainFiller";
import { tabsBtnsConnectToFunctions } from "./tabsBtnsConnectToFunctions";
import { initTrackPreview, updateTrackPreview } from "./trackPreview";

await init();

async function init() {
  await invoke("update_tracks_library");
  tabsBtnsConnectToFunctions();
  await initTrackPreview();

  await updateTrackPreview(null, null);

  await loadTracksFilerDatas();
}
