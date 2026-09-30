import { invoke } from "@tauri-apps/api/core";
import { Album } from "../bindings/Album";
import { PlayableResult } from "../bindings/PlayableResult";
import { addMediaToQueue } from "./playQueue";
import { contextMenuForAlbumInLib } from "./contextMenu";

export const albumTemplate = (albumUuid: String): string =>
  `<div class="playable-sellect-item album" data-uuid="${albumUuid}">
    <div class="album-cover">
        <img src="./no_cover.jpg">
    </div>
    <h4 class="album-title"></h4>
</div>`;

export const albumSet = (setName: string): string =>
  `
    <div class="playable-list albums-set">
    </div>
  `;

export async function fillAlbumsLibrary() {
  let albums_ids = await invoke<string[]>("get_playables_ids", {
    mediaType: "Album",
  }).catch(() => console.error("Error while getting albums ids"));


}
