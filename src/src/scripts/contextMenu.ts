import { invoke } from "@tauri-apps/api/core";
import { MediaType } from "../bindings/MediaType";
import { addMediaToQueue, startMedia } from "./playQueue";
import { fillPlaylistsList } from "./playlistsCreator";
import { PlayableResult } from "../bindings/PlayableResult";
import { Track } from "../bindings/Track";
import { pickFile } from "./filePicker";
import { Album } from "../bindings/Album";
import { Performer } from "../bindings/Performer";

const menu = (): string => `<context-menu></context-menu>`;


export async function contextMenuForTrackInLib(track_uuid: String, e: Event) {
  spawnMenu();
  setPos(e);

  await addToQueueButton("Track", track_uuid as string);
  await startBtn("Track", track_uuid as string);
  let track = (
    await invoke<PlayableResult>("get_playable_by_id", {
      mediaType: "Track",
      id: track_uuid,
    })
  ).data as Track;
  if (track.path == track.source_path) {
    await editMediaButton("Track", track_uuid as string);
  }
}

async function addToQueueButton(mt: MediaType, uuid: string) {
  (document.querySelector("context-menu") as HTMLElement).insertAdjacentHTML("beforeend", `<button id="context-add-to-queue"><h2>Add to queue</h2></button>`);
  document.querySelector("#context-add-to-queue")!.addEventListener("click", async () => {
    await addMediaToQueue(mt, uuid);
  })
}

async function editMediaButton(mt: MediaType, uuid: string) {
  (document.querySelector("context-menu") as HTMLElement).insertAdjacentHTML("beforeend", `<button id="context-edit-media"><h2>Edit media</h2></button>`);
  document.querySelector("#context-edit-media")!.addEventListener("click", async () => {
    await activateEditMediaMenu(uuid, mt);
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

  await editMediaButton("Album", album_uuid as string);
}

export async function contextMenuForPerformerInLib(performer_uuid: String, e: Event) {
  spawnMenu();
  setPos(e);

  await addToQueueButton("Performer", performer_uuid as string);
  await startBtn("Performer", performer_uuid as string);

  await editMediaButton("Performer", performer_uuid as string);

}

export async function contextMenuForTracksInQueue(track_uuid: String, e: Event) {
  spawnMenu();
  setPos(e);

  await jumpToTrack(track_uuid as string);
  await removeTrackFromQueue(track_uuid as string);

  let track = (
    await invoke<PlayableResult>("get_playable_by_id", {
      mediaType: "Track",
      id: track_uuid,
    })
  ).data as Track;
  if (track.path == track.source_path) {
    await editMediaButton("Track", track_uuid as string);
  }
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

function deactivateEditMediaMenu() {
  (document.querySelector("#blur-for-top")! as HTMLElement).remove();
  (document.querySelector(".section.top-section")! as HTMLElement).remove();
}

async function activateEditMediaMenu(mediaUuid: String, medaiaType: MediaType) {
  document.body.insertAdjacentHTML("beforeend", '<div id="blur-for-top"></div>');


  (document.querySelector("#blur-for-top")! as HTMLElement).addEventListener("click", () => deactivateEditMediaMenu());

  switch (medaiaType) {
    case "Track":
      await edit_track();
      break;
    case "Album":
      await edit_album();
      break;
    case "Performer":
      await edit_performer();
      break;
    case "Playlist":
      break;
  }

  async function edit_track() {
    document.body.insertAdjacentHTML("beforeend", '<section class="section top-section" id="track-edit-window"><div id="text-metadatas"></div><div id="not-text-metadatas"></div></section>');
    let topSection = document.querySelector(".section.top-section");
    let textData = topSection!.querySelector("#text-metadatas");
    let notTextData = topSection!.querySelector("#not-text-metadatas");
    topSection!.insertAdjacentHTML("beforeend", '<button id="apply-edit-media"><h2>Apply</h2></button>');
    let apply = topSection!.querySelector("#apply-edit-media");

    let track = (await invoke<PlayableResult>("get_playable_by_id", {
      mediaType: "Track",
      id: mediaUuid,
    })
    ).data as Track;

    textData!.insertAdjacentHTML("beforeend", '<h4>Title</h4>');
    textData!.insertAdjacentHTML("beforeend", `<input id="new-track-title" type="text" value="${track.title}" placeholder="Title">`);
    textData!.insertAdjacentHTML("beforeend", '<h4>Album</h4>');
    textData!.insertAdjacentHTML("beforeend", `<input id="new-track-album" type="text" value="${track.album}" placeholder="Album">`);
    textData!.insertAdjacentHTML("beforeend", '<h4>Performer</h4>');
    textData!.insertAdjacentHTML("beforeend", `<input id="new-track-performer" type="text" value="${track.performer}" placeholder="Performer">`);
    textData!.insertAdjacentHTML("beforeend", '<h4>Genre</h4>');
    textData!.insertAdjacentHTML("beforeend", `<input id="new-track-genre" type="text" value="${track.genre}" placeholder="Genre">`);

    let pathToCover: string | null = null;

    notTextData!.insertAdjacentHTML("beforeend", '<div class="album-cover" id="track-edit-cover"><img src="./src/assets/no_cover.jpg"></div>');
    let coverItem = notTextData!.querySelector("#track-edit-cover")!.lastChild as HTMLImageElement;
    await invoke<string>("get_track_cover", { id: mediaUuid })
      .then((cover) => {
        coverItem!.src = cover;
        if (cover == "") {
          coverItem!.src = "./src/assets/no_cover.jpg";
        }
      })
      .catch((error) => {
        console.error(error);
        coverItem!.src = "./src/assets/no_cover.jpg";
      });

    notTextData!.querySelector("#track-edit-cover")!.addEventListener("click", async () => {
      pathToCover = await pickFile("Sellect a cover", ["png", "jpg", "jpeg"]);
    })

    apply!.addEventListener("click", async () => {
      await invoke("edit_track", {
        uuid: mediaUuid,
        title: (textData!.querySelector("#new-track-title") as HTMLInputElement).value,
        album: (textData!.querySelector("#new-track-album") as HTMLInputElement).value,
        performer: (textData!.querySelector("#new-track-performer") as HTMLInputElement).value,
        genre: (textData!.querySelector("#new-track-genre") as HTMLInputElement).value,
        pathToCover: pathToCover,
      });
      await deactivateEditMediaMenu();
      await invoke("update_library");
    })
  }

  async function edit_album() {
    document.body.insertAdjacentHTML("beforeend", '<section class="section top-section" id="album-edit-window"></section>');
    let edit_window = document.querySelector("#album-edit-window");
    let topSection = document.querySelector(".section.top-section");
    topSection!.insertAdjacentHTML("beforeend", '<button id="apply-edit-media"><h2>Apply</h2></button>');
    let apply = topSection!.querySelector("#apply-edit-media");

    let album = (await invoke<PlayableResult>("get_playable_by_id", {
      mediaType: "Album",
      id: mediaUuid,
    })
    ).data as Album;

    edit_window!.insertAdjacentHTML("beforeend", '<h4>Title</h4>');
    edit_window!.insertAdjacentHTML("beforeend", `<input id="new-album-title" type="text" value="${album.title}" placeholder="Title">`);
    edit_window!.insertAdjacentHTML("beforeend", '<h4>Performer</h4>');
    edit_window!.insertAdjacentHTML("beforeend", `<input id="new-album-performer" type="text" value="${album.performer}" placeholder="Album">`);

    apply!.addEventListener("click", async () => {
      await invoke("edit_album", {
        uuid: mediaUuid,
        title: (edit_window!.querySelector("#new-album-title") as HTMLInputElement).value,
        performer: (edit_window!.querySelector("#new-album-performer") as HTMLInputElement).value,
      });
      await deactivateEditMediaMenu();
      await invoke("update_library");
    })
  }

  async function edit_performer() {
    document.body.insertAdjacentHTML("beforeend", '<section class="section top-section" id="album-edit-window"></section>');
    let edit_window = document.querySelector("#album-edit-window");
    let topSection = document.querySelector(".section.top-section");
    topSection!.insertAdjacentHTML("beforeend", '<button id="apply-edit-media"><h2>Apply</h2></button>');
    let apply = topSection!.querySelector("#apply-edit-media");

    let performer = (await invoke<PlayableResult>("get_playable_by_id", {
      mediaType: "Performer",
      id: mediaUuid,
    })
    ).data as Performer;

    edit_window!.insertAdjacentHTML("beforeend", '<h4>Title</h4>');
    edit_window!.insertAdjacentHTML("beforeend", `<input id="new-performer-title" type="text" value="${performer.title}" placeholder="Title">`);

    apply!.addEventListener("click", async () => {
      await invoke("edit_performer", {
        uuid: mediaUuid,
        title: (edit_window!.querySelector("#new-performer-title") as HTMLInputElement).value,
      });
      await deactivateEditMediaMenu();
      await invoke("update_library");
    })
  }
}
