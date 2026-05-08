import { invoke } from "@tauri-apps/api/core";
import { AlbumFilterMetadata } from "../bindings/AlbumFilterMetadata";
import { updateQueue } from "./queueFiller";

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
      await filterAlbumsList("");
    })
    .catch((e) => console.error(e));

  let searchField = document.querySelector("#search-album-field");
  searchField?.addEventListener("input", async (event) => {
    let value = (event.target as HTMLInputElement).value;
    await filterAlbumsList(value);
  });
}

async function filterAlbumsList(query: string) {
  let list = Array.from(
    document.querySelectorAll("#album-sellect-from-list-button"),
  );

  list.forEach((element) => {
    let isMatch =
      element
        ?.querySelector("h3")
        ?.textContent?.toLowerCase()
        .includes(query.toLowerCase()) ?? false;
    if (isMatch) {
      (element as HTMLEmbedElement).style.display = "";
    } else {
      (element as HTMLEmbedElement).style.display = "none";
    }
  });
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

  let add_btn = button.getElementById("add-playable-to-list");
  add_btn?.addEventListener("click", async () => {
    await invoke("add_album_by_id", { id: albumId });
    await updateQueue();
  });

  let cover = await invoke<String>("get_album_cover_by_id", { id: albumId });

  if (cover == "") {
    cover = "/black.jpg";
  }

  let coverElement = button.querySelector("img");
  let title = button.querySelector("h3");
  title!.textContent = data.title;
  coverElement!.src = cover.toString();

  return button;
}
