import { invoke } from "@tauri-apps/api/core";
import { TrackFilterMetadata } from "../bindings/TrackFilterMetadata";
import { updateQueue } from "./queueFiller";

export let tracksFilerDatas: Array<TrackFilterMetadata>;

const trackSellectButtonTempl = document.getElementById(
  "track-sellect-from-list-button-template",
) as HTMLTemplateElement;

export async function loadTracksFilerDatas() {
  await invoke<Array<TrackFilterMetadata>>("get_tracks_filter_datas")
    .then(async (data) => {
      tracksFilerDatas = data;
      await fillTracksList();
      await updateQueue();
      await filterTracksList("");
    })
    .catch((e) => console.error(e));

  let searchField = document.querySelector("#search-track-field");
  searchField?.addEventListener("input", async (event) => {
    let value = (event.target as HTMLInputElement).value;
    await filterTracksList(value);
  });
}

async function filterTracksList(query: string) {
  let list = Array.from(
    document.querySelectorAll("#track-sellect-from-list-button"),
  );
  list.forEach((element) => {
    let isMatch =
      element
        ?.querySelector("h2")
        ?.textContent?.toLowerCase()
        .includes(query.toLowerCase()) ?? false;
    if (isMatch) {
      (element as HTMLEmbedElement).style.display = "";
    } else {
      (element as HTMLEmbedElement).style.display = "none";
    }
  });
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
  let button = trackSellectButtonTempl.content.cloneNode(
    true,
  ) as DocumentFragment;

  let add_btn = button.getElementById("add-track-to-list-button");
  add_btn?.addEventListener("click", async () => {
    let id = data.id;
    console.log(id);

    await invoke("start_track", { id: id });
    await updateQueue();
  });

  let title = button.querySelector("h2");
  let performer = button.querySelector("h3");
  title!.textContent = data.title;
  performer!.textContent = data.artist;

  return button;
}
