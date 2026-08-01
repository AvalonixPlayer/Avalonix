import { invoke } from "@tauri-apps/api/core";
import { PlayableResult } from "../bindings/PlayableResult";
import { Track } from "../bindings/Track";
import { addMediaToQueue } from "./playQueue";
import { contextMenuForAlbumInLib, contextMenuForPerformerInLib, contextMenuForTrackInLib } from "./contextMenu";
import { trackTemplate } from "./tracksCreator";
import { albumTemplate } from "./albumsCreator";
import { Album } from "../bindings/Album";
import { Performer } from "../bindings/Performer";
import { performerTemplate } from "./performerCreator";


export async function initMediaSearch() {
  let line = document.querySelector("#media-search-line") as HTMLInputElement;

  fillTracks("");
  fillAlbums("");
  fillPerformers("");
  line!.addEventListener("input", async () => {
    fillTracks(line.value);
    fillAlbums(line.value);
    fillPerformers(line.value);
  });
}

async function fillTracks(partOfName: string) {
  const tracksObserver = new IntersectionObserver(
    (enteries, observer) => {
      enteries.forEach(async (entry) => {
        if (entry.isIntersecting) {
          const element = entry.target as HTMLElement;
          let uuid = element.getAttribute("data-uuid");
          let track = (
            await invoke<PlayableResult>("get_playable_by_id", {
              mediaType: "Track",
              id: uuid,
            })
          ).data as Track;

          let titleButton = element.querySelector(".track-title-button")!;
          titleButton.textContent = track.title;
          element.addEventListener("click", async () => {
            addMediaToQueue("Track", uuid!);
          });

          element.addEventListener("contextmenu", async (e) => {
            await contextMenuForTrackInLib(uuid!, e);
          })

          element.querySelector(".track-performer-button")!.textContent =
            track.performer;
          element.querySelector(".track-album-title-button")!.textContent =
            track.album;
          observer.unobserve(element);
        }
      });
    },
    {
      root: null,
      threshold: 0.1,
    },
  );

  let tracksIds = await invoke<string[]>("get_playables_ids_by_part_of_name", { partOfName: partOfName, mediaType: "Track" })
    .catch(() => console.error("Error while getting tracks ids"));

  let tracksList = document.querySelector("#media-search-tracks-list");

  tracksList!.innerHTML = "";
  tracksIds!.forEach((trackId) => {
    let element = trackTemplate(trackId);
    tracksList!.insertAdjacentHTML("beforeend", element);

    const lastInsertedElement = tracksList!.lastElementChild as HTMLElement;

    if (lastInsertedElement) {
      tracksObserver.observe(lastInsertedElement);
    }
  });
}

async function fillAlbums(partOfName: string) {
  const albumsObserver = new IntersectionObserver(
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

          let titleButton = element.querySelector(".album-title")!;
          titleButton.textContent = album.title;
          element.addEventListener("click", async () => {
            addMediaToQueue("Album", uuid!);
          });

          element.addEventListener("contextmenu", async (e) => {
            await contextMenuForAlbumInLib(uuid!, e);
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

  let albumsIds = await invoke<string[]>("get_playables_ids_by_part_of_name", { partOfName: partOfName, mediaType: "Album" })
    .catch(() => console.error("Error while getting tracks ids"));

  let albumsList = document.querySelector("#media-search-albums-list");

  albumsList!.innerHTML = "";
  albumsIds!.forEach((albumId) => {
    let element = albumTemplate(albumId);
    albumsList!.insertAdjacentHTML("beforeend", element);

    const lastInsertedElement = albumsList!.lastElementChild as HTMLElement;

    if (lastInsertedElement) {
      albumsObserver.observe(lastInsertedElement);
    }
  });
}

async function fillPerformers(partOfName: string) {
  const performersObserver = new IntersectionObserver(
    (enteries, observer) => {
      enteries.forEach(async (entry) => {
        if (entry.isIntersecting) {
          const element = entry.target as HTMLElement;
          let uuid = element.getAttribute("data-uuid");
          let performer = (
            await invoke<PlayableResult>("get_playable_by_id", {
              mediaType: "Performer",
              id: uuid,
            })
          ).data as Performer;

          let titleButton = element.querySelector(".performer-title-button")!;
          titleButton.textContent = performer.title;
          element.addEventListener("click", async () => {
            addMediaToQueue("Track", uuid!);
          });

          element.addEventListener("contextmenu", async (e) => {
            await contextMenuForPerformerInLib(uuid!, e);
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

  let performersIds = await invoke<string[]>("get_playables_ids_by_part_of_name", { partOfName: partOfName, mediaType: "Performer" })
    .catch(() => console.error("Error while getting tracks ids"));

  let performersList = document.querySelector("#media-search-performers-list");

  performersList!.innerHTML = "";
  performersIds!.forEach((performerId) => {
    let element = performerTemplate(performerId);
    performersList!.insertAdjacentHTML("beforeend", element);

    const lastInsertedElement = performersList!.lastElementChild as HTMLElement;

    if (lastInsertedElement) {
      performersObserver.observe(lastInsertedElement);
    }
  });
}
