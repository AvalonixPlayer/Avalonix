import { loadTracksFilerDatas } from "./mainFiller";
import { tabsBtnsConnectToFunctions } from "./tabsBtnsConnectToFunctions";

await init();

async function init() {
  tabsBtnsConnectToFunctions();
  await loadTracksFilerDatas();
}
