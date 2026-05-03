import { invoke } from "@tauri-apps/api/core";
import { AlbumFilterMetadata } from "../bindings/AlbumFilterMetadata";

export let albumsFilerDatas: Array<AlbumFilterMetadata>;
let albumsIds: Array<Array<number>>;

const albumSellectButtonTempl = document.getElementById(
  "album-sellect-from-list-button-template",
) as HTMLTemplateElement;

export async function loadAlbumsFilerDatas() {
  await invoke<Array<AlbumFilterMetadata>>("get_albums_filter_datas")
    .then(async (data) => {
      albumsFilerDatas = data;
      albumsIds = await invoke<Array<Array<number>>>("get_albums_ids");
      await fillAlbumsList();
    })
    .catch((e) => console.error(e));
}

export async function fillAlbumsList() {
  let list = document.querySelector("#albums-list");
  list!.innerHTML = "";
  albumsFilerDatas.forEach(async (data, index) => {
    let btn = await createButton(data, albumsIds[index]);
    list?.append(btn);
  });
}

async function createButton(data: AlbumFilterMetadata, albumId: Array<number>) {
  let button = albumSellectButtonTempl.content.cloneNode(
    true,
  ) as DocumentFragment;

  let add_btn = button.getElementById("add-album-to-list-button");
  add_btn?.addEventListener("click", async () => {
    let index = albumsFilerDatas.findIndex((d) => d === data);
  });

  let cover = await invoke<String>("get_album_cover_by_id", { id: albumId });

  if (cover == "") {
    cover = "src/assets/black.jpg";
  }

  let coverElement = button.querySelector("img");
  let title = button.querySelector("h3");
  title!.textContent = data.title;
  coverElement!.src = cover.toString();

  return button;
}
