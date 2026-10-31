import { invoke } from "@tauri-apps/api/core";
import { addMediaToQueue } from "./playQueue";
import { Playlist } from "../bindings/Playlist";
import { PlayableResult } from "../bindings/PlayableResult";


let playlistTemplate = (playlist_uuid: string): string =>
  `<div class="button playable playlist-button" data-uuid="${playlist_uuid}">
  <div>
    <h4 class="playlist-title"></h4>
  </div>
  <div>
  <h4 class="playlist-len"></h4>
  </div>
</div>`;

export async function fillPlaylistsLibrary() {
  let sortBy = "ByName";

  let createNewPlaylistButton = document.getElementById("create-new-playlist");
  let createNewPlaylistTextField = document.getElementById("new-playlist-name") as HTMLInputElement;
  createNewPlaylistButton?.addEventListener("click", async () => {
    await invoke("create_playlist", {playlistName: createNewPlaylistTextField.value});
    await spawn(sortBy);
  })

  await spawn(sortBy);
}

async function spawn(sortBy: string) {
  let playlistsSet = await invoke<Record<string, string[]>>("get_playables_ids", {
    mediaType: "Playlist",
    sortBy: {SortPerformers: sortBy}
  }).catch(() => console.error("Error while getting playlists in library ids"));

  let playlists = document.getElementById("playlists-list");
  playlists!.innerHTML = "";

  const observer = new IntersectionObserver(
    (enteries, observer) => {
      enteries.forEach(async (entry) => {
        if (entry.isIntersecting) {
          const element = entry.target as HTMLElement;
          let uuid = element.getAttribute("data-uuid");
          let playlist = (
            await invoke<PlayableResult>("get_playable_by_id", {
              mediaType: "Playlist",
              id: uuid,
            })
          ).data as Playlist;

          let playlistTitle = element.querySelector(".playlist-title")!;
          playlistTitle.textContent = playlist.title;
          let len = element.querySelector(".playlist-len")!;
          len.textContent = playlist.tracks_ids.length.toString();

          element.addEventListener("click", async () => {
            await addMediaToQueue("Playlist", uuid!);
          });

          observer.unobserve(element);
        }
      });
    },
    {
      threshold: 0.1,
    },

  );


  const allIds = playlistsSet ? Object.values(playlistsSet).flat() : [];

  allIds?.forEach(id => {
    let playlistButton = playlistTemplate(id);

    playlists!.insertAdjacentHTML("beforeend", playlistButton);

    const lastInsertedElement = playlists!.lastElementChild as HTMLElement;

    if (lastInsertedElement) {
      observer.observe(lastInsertedElement)
    }
  });
}
