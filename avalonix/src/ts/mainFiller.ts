import { invoke } from "@tauri-apps/api/core";
import { TrackFilterMetadata } from "../bindings/TrackFilterMetadata";

let tracksFilerDatas: Array<TrackFilterMetadata>;

const trackSellectButtonTempl = document.getElementById(
  "track-sellect-from-list-button-template",
) as HTMLTemplateElement;

export async function loadTracksFilerDatas() {
  await invoke<Array<TrackFilterMetadata>>("get_tracks_filter_datas")
    .then((data) => {
      tracksFilerDatas = data;
      fillTracksList();
    })
    .catch((e) => console.error(e));
}

export async function fillTracksList() {
  let list = document.querySelector("#tracks-list");
  list!.innerHTML = "";
  tracksFilerDatas.forEach((data) => {
    console.log(data);
    let btn = createButton(data);
    list?.append(btn);
  });
}

function createButton(data: TrackFilterMetadata) {
  console.log(trackSellectButtonTempl);
  let button = trackSellectButtonTempl.content.cloneNode(
    true,
  ) as DocumentFragment;

  let title = button.querySelector("h2");
  let performer = button.querySelector("h3");
  title!.textContent = data.title;
  performer!.textContent = data.artist;

  return button;
}
