import { invoke } from "@tauri-apps/api/core";
import { Album } from "../bindings/Album";
import { PlayableResult } from "../bindings/PlayableResult";
import { addMediaToQueue } from "./playQueue";
import { contextMenuForAlbumInLib } from "./contextMenu";

export const albumTemplate = (albumUuid: String): string =>
  `<div class="button playable album-button" data-uuid="${albumUuid}">
    <img class="cover album cover-in-library" src="">
    <div>
      <h4 class="album-title"></h4>
    </div>
  </div>`;

export const albumSetTemplate = (setName: string): string =>
  `
    <div class="playable-list albums-set">
    <h2>${setName}</h2>
    <div>
    </div>
    </div>
  `;

export async function fillAlbumsLibrary() {
  let sortModPicker = document.getElementById("albums-sort-select") as HTMLSelectElement;

  let sortBy = "ByName";
  sortModPicker.addEventListener("change", async (e) => {
    sortBy = (e.target as HTMLSelectElement).value;
    await spawn(sortBy);
  });
  await spawn(sortBy);
}

async function spawn(sortBy: string) {
  let albums_ids = await invoke<Record<string, string[]>>("get_playables_ids", {
    mediaType: "Album",
    sortBy: {SortAlbums: sortBy}
  }).catch(() => console.error("Error while getting albums ids"));

  let list = document.getElementById("albums-list");
  list!.innerHTML = "";

  const observer = new IntersectionObserver(
    (enteries, observer) => {
      enteries.forEach(async (entry) => {
        if (entry.isIntersecting) {
          const element = entry.target as HTMLElement;
          let uuid = element.getAttribute("data-uuid");
          let album = (
            await invoke<PlayableResult>("get_playable_by_id", {
              mediaType: "Album",
              id: uuid,
            })
          ).data as Album;

          let albumCover: string = album.cover_uri;

          let albumCoverElement = element.querySelector("img") as HTMLImageElement;

          let titleElement = element.querySelector("h4") as Element;
          titleElement.textContent = album.title;

          albumCoverElement.animate([
            { opacity: 0, transform: 'translateX(-20px)'},
            { opacity: 1, transform: 'translateX(0)'}
          ], {
            duration: 1400,
            easing: 'ease',
            fill: 'forwards'
          });

          albumCoverElement.src = albumCover;

          albumCoverElement.onerror = () => { albumCoverElement.classList.add('broken-cover'); };

          element.addEventListener("click", async () => {
            await addMediaToQueue("Album", uuid!);
          });

          observer.unobserve(element);
        }
      });
    },
    {
      threshold: 0.1,
    },
  );

  for (let i = 0; i < Object.keys(albums_ids!).length; i++) {
    let key = Object.keys(albums_ids!)[i];
    let htmlSet = albumSetTemplate(key);

    list!.insertAdjacentHTML("beforeend", htmlSet);

    let albumsList = (list?.lastElementChild as HTMLElement).querySelector("div");

    albums_ids![key]!.forEach((id) => {
      let album = albumTemplate(id);
      albumsList!.insertAdjacentHTML("beforeend", album);

      const lastInsertedElement = albumsList!.lastElementChild as HTMLElement;

      observer.observe(lastInsertedElement);
    });

  }


}
