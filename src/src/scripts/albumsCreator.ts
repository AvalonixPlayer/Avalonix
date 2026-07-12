import { invoke } from "@tauri-apps/api/core";
import { Album } from "../bindings/Album";
import { PlayableResult } from "../bindings/PlayableResult";
import { addMediaToQueue } from "./playQueue";

const albumTemplate = (album_uuid: String): string =>
  `<div class="playable-sellect-item album" data-uuid="${album_uuid}">
    <div class="album-cover">
        <img src="./src/assets/no_cover.jpg">
    </div>
    <h4 class="album-title"></h4>
</div>`;

const albumSetTemplate = (
  first_album_uuid: String,
  performer_name: String,
): string => `
  <div class="albums-set" data-performer-name="${performer_name}">
      <h2>${performer_name}</h2>
      <div>
          ${albumTemplate(first_album_uuid)}
      </div>
  </div>`;

export async function fillAlbumsList() {
  let albums_ids = await invoke<string[]>("get_playables_ids", {
    mediaType: "Album",
  }).catch(() => console.error("Error while getting albums ids"));

  if (albums_ids == null) {
    return;
  }

  console.log(`ids: ${albums_ids}`);

  let albumsList = document.getElementById("albums-list-section");
  albumsList!.innerHTML = "";

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
          if (album.cover_uri.length != 0) {
            element.querySelector("img")!.src = album.cover_uri;
            element.animate([{ opacity: 0 }, { opacity: 1 }], {
              duration: 500,
              fill: "forwards",
            });
          }

          let albumTitleButton = element.querySelector(".album-title")!;
          albumTitleButton.textContent = album.title;
          albumTitleButton.addEventListener("click", async () => {
            addMediaToQueue("Album", uuid!);
          });
          observer.unobserve(element);
        }
      });
    },
    {
      root: null,
      threshold: 0.1,
    },
  );

  albums_ids!.forEach(async (album_id) => {
    let album_performer_name = await invoke<string>(
      "get_album_performer_name_by_id",
      {
        id: album_id,
      },
    );

    let albumSet = albumsList!.querySelector(
      `[data-performer-name="${album_performer_name}"]`,
    );

    if (albumSet == null) {
      albumsList!.insertAdjacentHTML(
        "beforeend",
        albumSetTemplate(album_id, album_performer_name),
      );
    } else {
      albumSet!
        .querySelector("div")!
        .insertAdjacentHTML("beforeend", albumTemplate(album_id));
    }

    let albumSetNew = albumsList!.querySelector(
      `[data-performer-name="${album_performer_name}"]`,
    );

    observer.observe(
      albumSetNew!.lastElementChild!.lastElementChild as HTMLElement,
    );
  });
}
