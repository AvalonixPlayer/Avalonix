import { invoke } from "@tauri-apps/api/core";
import { Album } from "../bindings/Album";

export async function fillAlbumsList() {
  let albums = await invoke<Album[]>("get_albums_datas").catch(() =>
    console.error("Error while getting tracks"),
  );

  if (albums == null) {
    return;
  }

  let albumsList = document.getElementById("albums-list-section");

  albums.forEach((album) => {
    let set = document.querySelector(
      `.albums-set[data-performer="${album.performer}"]`,
    );
    if (set == null) {
      albumsList!.insertAdjacentHTML("beforeend", albumSetTemplate(album));
    } else {
      set!
        .querySelector("div")!
        .insertAdjacentHTML("beforeend", albumTemplate(album));
    }
  });
}

const albumSetTemplate = (album: Album): string => `
  <div class="albums-set" data-performer="${album.performer}">
      <h2>${album.performer}</h2>
      <div>
          ${albumTemplate(album)}
      </div>
  </div>`;

const albumTemplate = (
  album: Album,
): string => `<div class="playable-sellect-item album">
    <div class="album-cover">
        <img src="./src/assets/black.jpg">
    </div>
    <h3>${album.title}</h3>
</div>`;
