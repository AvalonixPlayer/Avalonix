import { invoke } from "@tauri-apps/api/core";
import { updateQueue } from "./queueFiller";

const albumContextMenuTemplate = document.querySelector(
  "#context-menu-for-album-template",
) as HTMLTemplateElement;

export function initContextMenu() {
  document.addEventListener("contextmenu", (e) => {
    e.preventDefault();
  });
  window.addEventListener("click", () => {
    removeExistsContextMenus();
  });
}

export async function invokeAlbumPlayableContextMenu(
  pointerEvent: PointerEvent,
  albumId: Array<number>,
) {
  pointerEvent.preventDefault();
  removeExistsContextMenus();

  let menu = albumContextMenuTemplate.content.cloneNode(
    true,
  ) as DocumentFragment;
  setPosition(pointerEvent, menu);

  let playButton = menu.querySelector(".play-playable");
  playButton!.addEventListener("click", async () => {
    await invoke("play_album_by_id", { id: albumId });
    await updateQueue();
  });

  let addButton = menu.querySelector(".add-playable-to-queue");
  addButton!.addEventListener("click", async () => {
    await invoke("add_album_by_id", { id: albumId });
    await updateQueue();
  });

  document.body.appendChild(menu);
}

function removeExistsContextMenus() {
  let existsMenus = document.querySelectorAll(".context-menu");
  existsMenus.forEach((element) => {
    element.remove();
  });
}

function setPosition(pointerEvent: PointerEvent, menu: DocumentFragment) {
  let element = menu.firstElementChild as HTMLElement;

  let posX = pointerEvent.clientX;
  let posY = pointerEvent.clientY;
  element.style.left = posX + "px";
  element.style.top = posY + "px";
}
