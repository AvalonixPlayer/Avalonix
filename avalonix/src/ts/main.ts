import { invoke } from "@tauri-apps/api/core";
import { loadTracksFilerDatas } from "./mainFiller";
import { tabsBtnsConnectToFunctions } from "./tabsBtnsConnectToFunctions";
import { initTrackPreview, updateTrackPreview } from "./trackPreview";
import { initPlaybackControll } from "./playbackControll";
import { loadAlbumsFilerDatas } from "./loadAlbumsFilterDatas";

await init();

async function init() {
  await invoke("update_tracks_library");
  tabsBtnsConnectToFunctions();
  await initPlaybackControll();
  await initTrackPreview();
  await updateTrackPreview(null, null);
  await loadTracksFilerDatas();
  await loadAlbumsFilerDatas();
}
