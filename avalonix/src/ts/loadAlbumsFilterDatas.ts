import { invoke } from "@tauri-apps/api/core";
import { AlbumFilterMetadata } from "../bindings/AlbumFilterMetadata";

export let albumsFilerDatas: Array<AlbumFilterMetadata>;

const albumSellectButtonTempl = document.getElementById(
  "album-sellect-from-list-button-template",
) as HTMLTemplateElement;

export async function loadAlbumsFilerDatas() {
  await invoke<Array<AlbumFilterMetadata>>("get_albums_filter_datas")
    .then(async (data) => {
      albumsFilerDatas = data;
      await fillAlbumsList();
    })
    .catch((e) => console.error(e));
}

export async function fillAlbumsList() {
  let list = document.querySelector("#albums-list");
  list!.innerHTML = "";
  albumsFilerDatas.forEach(async (data) => {
    let btn = await createButton(data);
    list?.append(btn);
  });
}

async function createButton(data: AlbumFilterMetadata) {
  let button = albumSellectButtonTempl.content.cloneNode(
    true,
  ) as DocumentFragment;

  let add_btn = button.getElementById("add-album-to-list-button");
  add_btn?.addEventListener("click", async () => {
    let index = albumsFilerDatas.findIndex((d) => d === data);
    console.log(index);
  });

  let title = button.querySelector("h3");
  title!.textContent = data.title;

  return button;
}
