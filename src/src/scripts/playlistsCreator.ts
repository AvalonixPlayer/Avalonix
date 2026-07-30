import { invoke } from "@tauri-apps/api/core";
import { PlayableResult } from "../bindings/PlayableResult";
import { Playlist } from "../bindings/Playlist";
import { addMediaToQueue } from "./playQueue";
import { contextMenuForPlaylist } from "./contextMenu";

let playlistTemplate = (playlist_uuid: string): string =>
  `<div class="playable-sellect-item playlist" data-uuid="${playlist_uuid}">
  <h3 class="playlist-title-button"></h3>
</div>`;

export async function createPlaylistButton() {
  let name = document.querySelector("#create-playlist-name") as HTMLInputElement;
  let button = document.querySelector("#create-playlist-button");

  button!.addEventListener("click", async () => {
    await invoke("create_playlist", { playlistName: name.value });
    await fillPlaylistsList();
    name.value = "";
  });
}

export async function fillPlaylistsList() {
  let playlists_ids = await invoke<string[]>("get_playables_ids", {
    mediaType: "Playlist",
  }).catch(() => console.error("Error while getting playlists ids"));

  if (playlists_ids == null) {
    return;
  }

  let playlistsList = document.getElementById("playlists");
  playlistsList!.innerHTML = "";

  const observer = new IntersectionObserver(
    (enteries, observer) => {
      enteries!.forEach(async (entry) => {
        if (entry.isIntersecting) {
          const element = entry.target as HTMLElement;
          let uuid = element.getAttribute("data-uuid");
          let playlist = (
            await invoke<PlayableResult>("get_playable_by_id", {
              mediaType: "Playlist",
              id: uuid,
            })
          ).data as Playlist;

          let playlistTitleButton = element.querySelector(
            ".playlist-title-button",
          )!;
          playlistTitleButton.textContent = playlist.title;

          playlistTitleButton.addEventListener("click", async () => {
            addMediaToQueue("Playlist", uuid!);
          });

          playlistTitleButton.addEventListener("contextmenu", async (e) => {
            await contextMenuForPlaylist(uuid!, e);
          })
          observer.unobserve(element);
        }
      });
    },
    {
      root: null,
      threshold: 0.1,
    },
  );

  playlists_ids.forEach((performer_id) => {
    let element = playlistTemplate(performer_id);
    playlistsList!.insertAdjacentHTML("beforeend", element);

    const lastInsertedElement = playlistsList!.lastElementChild as HTMLElement;

    if (lastInsertedElement) {
      observer.observe(lastInsertedElement);
    }
  });
}
