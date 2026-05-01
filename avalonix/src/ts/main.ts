import { loadTracksFilerDatas } from "./mainFiller";
import { tabsBtnsConnectToFunctions } from "./tabsBtnsConnectToFunctions";
import { initTrackPreview, updateTrackPreview } from "./trackPreview";

await init();

async function init() {
  tabsBtnsConnectToFunctions();
  await initTrackPreview();

  await updateTrackPreview(null, null);

  await loadTracksFilerDatas();
}
