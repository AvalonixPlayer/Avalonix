import { invoke } from "@tauri-apps/api/core";
import { TrackFilterMetadata } from "../bindings/TrackFilterMetadata";
import { updateQueue } from "./queueFiller";

export let tracksFilerDatas: Array<TrackFilterMetadata>;

const trackSellectButtonTempl = document.getElementById(
  "track-sellect-from-list-button-template",
) as HTMLTemplateElement;

export async function loadTracksFilerDatas() {
  await invoke<Array<TrackFilterMetadata>>("get_tracks_filter_datas")
    .then((data) => {
      tracksFilerDatas = data;
      fillTracksList();
      updateQueue();
    })
    .catch((e) => console.error(e));
}

export async function fillTracksList() {
  let list = document.querySelector("#tracks-list");
  list!.innerHTML = "";
  tracksFilerDatas.forEach(async (data) => {
    let btn = await createButton(data);
    list?.append(btn);
  });
}

async function createButton(data: TrackFilterMetadata) {
  console.log(trackSellectButtonTempl);
  let button = trackSellectButtonTempl.content.cloneNode(
    true,
  ) as DocumentFragment;

  let add_btn = button.getElementById("add-track-to-list-button");
  add_btn?.addEventListener("click", async () => {
    let index = tracksFilerDatas.findIndex((d) => d === data);
    console.log(index);

    await invoke("start_track", { index: index });
    await updateQueue();
  });

  let title = button.querySelector("h2");
  let performer = button.querySelector("h3");
  title!.textContent = data.title;
  performer!.textContent = data.artist;

  return button;
}
