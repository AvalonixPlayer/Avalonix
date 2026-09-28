import { invoke } from "@tauri-apps/api/core";
import { PlayableResult } from "../bindings/PlayableResult";
import { Track } from "../bindings/Track";
import { addMediaToQueue } from "./playQueue";

const trackButtonTemplate = (track_uuid: string): string =>
  `<div class="button playable track-button" data-uuid="${track_uuid}">
    <h4 class="track-title"></h3>
    <h5 class="track-performer"></h4>
  </div>`;

export async function fillTracksLibrary() {
  let tracksIds = await invoke<string[]>("get_playables_ids", {
    mediaType: "Track",
  }).catch(() => console.error("Error while getting tracks in library ids"));

  let tracksList = document.getElementById("tracks-list");
  tracksList!.innerHTML = "";

  const observer = new IntersectionObserver(
    (enteries, observer) => {
      enteries.forEach(async (entry) => {
        if (entry.isIntersecting) {
          const element = entry.target as HTMLElement;
          console.log(element);
          let uuid = element.getAttribute("data-uuid");
          let track = (
            await invoke<PlayableResult>("get_playable_by_id", {
              mediaType: "Track",
              id: uuid,
            })
          ).data as Track;

          let trackTitle = element.querySelector(".track-title")!;
          trackTitle.textContent = track.title;
          let trackPerformer = element.querySelector(".track-performer")!;
          trackPerformer.textContent = track.performer;

          element.addEventListener("click", async () => {
            await addMediaToQueue("Track", uuid!);
          });
          observer.unobserve(element);
        }
      });
    },
    {
      threshold: 0.1,
    },

  );

  tracksIds?.forEach(id => {
    let trackButton = trackButtonTemplate(id);

    tracksList!.insertAdjacentHTML("beforeend", trackButton);

    const lastInsertedElement = tracksList!.lastElementChild as HTMLElement;

    if (lastInsertedElement) {
      observer.observe(lastInsertedElement)
    }
  });
}
