import { invoke } from "@tauri-apps/api/core";
import { updateQueue } from "./queueFiller";
import { PerformerFilterMetadata } from "../bindings/PerformerFilterMetadata";

export let performersFilerDatas: Array<PerformerFilterMetadata>;
let performersIds: Array<Array<number>>;

const performerSellectButtonTempl = document.getElementById(
  "performer-sellect-from-list-button-template",
) as HTMLTemplateElement;

export async function loadPerformersFilerDatas() {
  await invoke<Array<PerformerFilterMetadata>>("get_performers_filter_datas")
    .then(async (data) => {
      performersFilerDatas = data;

      performersIds = await invoke<Array<Array<number>>>("get_performers_ids");
      await fillPerformersList();
      await filterPerformersList("");
    })
    .catch((e) => console.error(e));

  let searchField = document.querySelector("#search-performer-field");
  searchField?.addEventListener("input", async (event) => {
    let value = (event.target as HTMLInputElement).value;
    await filterPerformersList(value);
  });
}

async function filterPerformersList(query: string) {
  let list = Array.from(
    document.querySelectorAll("#performer-sellect-from-list-button"),
  );

  list.forEach((element) => {
    let isMatch = element
      .querySelector("h1")!
      .textContent.toLowerCase()
      .includes(query);
    if (isMatch) {
      (element as HTMLEmbedElement).style.display = "";
    } else {
      (element as HTMLEmbedElement).style.display = "none";
    }
  });
}

export async function fillPerformersList() {
  let list = document.querySelector("#performers-list");
  list!.innerHTML = "";
  performersFilerDatas.forEach(async (data, index) => {
    let btn = await createButton(data, performersIds[index]);
    list?.append(btn);
  });
}

async function createButton(
  data: PerformerFilterMetadata,
  performerId: Array<number>,
) {
  let button = performerSellectButtonTempl.content.cloneNode(
    true,
  ) as DocumentFragment;

  let add_btn = button.getElementById("add-performer-to-list-button");
  add_btn?.addEventListener("click", async () => {
    await invoke("add_performer_by_id", { id: performerId });
    await updateQueue();
  });

  let title = button.querySelector("h1");
  title!.textContent = data.title;

  return button;
}
