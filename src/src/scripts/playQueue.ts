import { invoke } from "@tauri-apps/api/core";
import { MediaType } from "../bindings/MediaType";
import { Track } from "../bindings/Track";
import { PlayableResult } from "../bindings/PlayableResult";

const trackTemplate = (uuid: string): string => `
  <div class="playable-sellect-item track" data-uuid=${uuid}>
    <h3 class="track-title-button"></h3>
    <h3 class="track-performer-button"></h3>
    <h3 class="track-album-title-button"></h3>
  </div>`;

export async function clearPlayQueue() {
  await invoke("clear_queue");
}

export async function addMediaToQueue(mediaType: MediaType, id: string) {
  switch (mediaType) {
    case "Track":
      break;
    case "Album":
      await clearPlayQueue();
      break;
    case "Performer":
      await clearPlayQueue();
      break;
  }

  await invoke("add_media_to_queue", {
    mediaType: mediaType.toString(),
    id,
  });
}

export async function fillPlayQueueList() {
  let tracksList = document.getElementById("queue-section");
  tracksList!.innerHTML = "";

  const observer = new IntersectionObserver(
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

          let timer = setTimeout(() => {});
          const delay = 100;

          let titleButton = element.querySelector(".track-title-button")!;
          titleButton.addEventListener("click", async () => {
            timer = setTimeout(async () => { await invoke("start_track_in_queue_by_id", {id: uuid});}, delay);
          });
          titleButton.addEventListener("dblclick", async () => {
            clearTimeout(timer);
            await invoke("remove_track_from_queue_by_id", {id: uuid});
          });

          titleButton.textContent = track.title;

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

  (await invoke<string[]>("get_queue_tracks_ids")).forEach((track_id) => {
    let element = trackTemplate(track_id);
    tracksList!.insertAdjacentHTML("beforeend", element);

    const lastInsertedElement = tracksList!.lastElementChild as HTMLElement;

    if (lastInsertedElement) {
      observer.observe(lastInsertedElement);
    }
  });
}
