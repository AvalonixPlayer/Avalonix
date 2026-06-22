import { invoke } from "@tauri-apps/api/core";
import { Track } from "../bindings/Track";
import { PlayableResult } from "../bindings/PlayableResult";
import { addMediaToQueue } from "./playQueue";

const trackTemplate = (uuid: string): string => `
  <div class="playable-sellect-item track" data-uuid=${uuid}>
    <h3 class="track-title-button"></h3>
    <h3 class="track-performer-button"></h3>
    <h3 class="track-album-title-button"></h3>
  </div>`;

export async function fillTracksList() {
  let tracks_ids = await invoke<string[]>("get_playables_ids", {
    mediaType: "Track",
  }).catch(() => console.error("Error while getting tracks ids"));

  if (tracks_ids == null) {
    return;
  }

  let tracksList = document.getElementById("tracks-list-section");
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

          let titleButton = element.querySelector(".track-title-button")!;
          titleButton.textContent = track.title;
          titleButton.addEventListener("click", async () => {
            addMediaToQueue("Track", uuid!);
          });

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

  tracks_ids!.forEach((track_id) => {
    let element = trackTemplate(track_id);
    tracksList!.insertAdjacentHTML("beforeend", element);

    const lastInsertedElement = tracksList!.lastElementChild as HTMLElement;

    if (lastInsertedElement) {
      observer.observe(lastInsertedElement);
    }
  });
}
