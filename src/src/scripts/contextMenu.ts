import { invoke } from "@tauri-apps/api/core";
import { MediaType } from "../bindings/MediaType";
import { addMediaToQueue, startMedia } from "./playQueue";
import { fillPlaylistsList } from "./playlistsCreator";

const menu = (): string => `<context-menu></context-menu>`;


export async function contextMenuForTrackInLib(track_uuid: String, e: Event) {
  spawnMenu();
  setPos(e);

  await addToQueueButton("Track", track_uuid as string);
  await startBtn("Track", track_uuid as string);
}

async function addToQueueButton(mt: MediaType, uuid: string) {
  (document.querySelector("context-menu") as HTMLElement).insertAdjacentHTML("beforeend", `<button id="context-add-to-queue"><h2>Add to queue</h2></button>`);
  document.querySelector("#context-add-to-queue")!.addEventListener("click", async () => {
    await addMediaToQueue(mt, uuid);
  })
}

async function startBtn(mt: MediaType, uuid: string) {
  (document.querySelector("context-menu") as HTMLElement).insertAdjacentHTML("beforeend", `<button id="context-start"><h2>Start media</h2></button>`);
  document.querySelector("#context-start")!.addEventListener("click", async () => {
    await startMedia(mt, uuid);
  })
}

async function removePlaylistBtn(uuid: string) {
  (document.querySelector("context-menu") as HTMLElement).insertAdjacentHTML("beforeend", `<button id="context-remove"><h2>Remove media</h2></button>`);
  document.querySelector("#context-remove")!.addEventListener("click", async () => {
    await invoke("remove_playlist", { playlistUuid: uuid });
    await fillPlaylistsList();
  })
}

async function jumpToTrack(uuid: string) {
  (document.querySelector("context-menu") as HTMLElement).insertAdjacentHTML("beforeend", `<button id="context-jump"><h2>Start media</h2></button>`);
  document.querySelector("#context-jump")!.addEventListener("click", async () => {
    await invoke("start_track_in_queue_by_id", { id: uuid });
  })
}

async function removeTrackFromQueue(uuid: string) {
  (document.querySelector("context-menu") as HTMLElement).insertAdjacentHTML("beforeend", `<button id="context-remove"><h2>Remove media</h2></button>`);
  document.querySelector("#context-remove")!.addEventListener("click", async () => {
    await invoke("remove_track_from_queue_by_id", {id: uuid});
  })
}

export async function contextMenuForAlbumInLib(album_uuid: String, e: Event) {
  spawnMenu();
  setPos(e);

  await addToQueueButton("Album", album_uuid as string);
  await startBtn("Album", album_uuid as string);
}

export async function contextMenuForPerformerInLib(performer_uuid: String, e: Event) {
  spawnMenu();
  setPos(e);

  await addToQueueButton("Performer", performer_uuid as string);
  await startBtn("Performer", performer_uuid as string);
}

export async function contextMenuForTracksInQueue(track_uuid: String, e: Event) {
  spawnMenu();
  setPos(e);

  await jumpToTrack(track_uuid as string);
  await removeTrackFromQueue(track_uuid as string);
}

export async function contextMenuForPlaylist(playlist_uuid: String, e: Event) {
  spawnMenu();
  setPos(e);

  await addToQueueButton("Playlist", playlist_uuid as string);
  await startBtn("Playlist", playlist_uuid as string);
  await removePlaylistBtn(playlist_uuid as string);
}

function spawnMenu() {
  let elements = document.querySelector("context-menu");
  elements?.remove();
  document.body.insertAdjacentHTML("beforeend", menu());
}

function setPos(e: Event) {
  let e2 = e as MouseEvent;
  let menu = document.querySelector("context-menu") as HTMLElement;
  menu.style.top = `${e2.clientY - 5}px`;
  menu.style.left = `${e2.clientX - 5}px`;
  document.addEventListener("click", () => {
    let elements = document.querySelector("context-menu");
    elements?.remove();
  })
}
