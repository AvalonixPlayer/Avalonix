import { invoke } from "@tauri-apps/api/core";
import { PlayableResult } from "../bindings/PlayableResult";
import { Track } from "../bindings/Track";
import { addMediaToQueue } from "./playQueue";

const trackButtonTemplate = (track_uuid: string): string =>
  `<div class="button playable track-button" data-uuid="${track_uuid}">
    <img class="cover track cover-in-library" src="">
    <div>
      <h4 class="track-title"></h3>
      <h5 class="track-performer"></h4>
    </div>
    <div>
      <h5 class="track-genre"></h5>
    </div>
  </div>`;

export async function fillTracksLibrary() {
  let sortModPicker = document.getElementById("tracks-sort-select") as HTMLSelectElement;

  let sortBy = "ByName";
  sortModPicker.addEventListener("change", async (e) => {
    sortBy = (e.target as HTMLSelectElement).value;
    await spawn(sortBy);
  });
  await spawn(sortBy);
}

async function spawn(sortBy: string) {
  let tracksSets = await invoke<Record<string, string[]>>("get_playables_ids", {
    mediaType: "Track",
    sortBy: {SortTracks: sortBy}
  }).catch(() => console.error("Error while getting tracks in library ids"));

  let tracksList = document.getElementById("tracks-list");
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

          let trackTitle = element.querySelector(".track-title")!;
          trackTitle.textContent = track.title;
          let trackPerformer = element.querySelector(".track-performer")!;
          trackPerformer.textContent = track.performer;
          let trackGenre = element.querySelector(".track-genre")!;
          trackGenre.textContent = track.genre;

          element.addEventListener("click", async () => {
            await addMediaToQueue("Track", uuid!);
          });

          let trackCover: string = await invoke("get_track_cover", { id: uuid });

          let trackCoverElement = element.querySelector("img") as HTMLImageElement;

          trackCoverElement.animate([
            { opacity: 0, transform: 'translateX(-20px)'},
            { opacity: 1, transform: 'translateX(0)'}
          ], {
            duration: 1400,
            easing: 'ease',
            fill: 'forwards'
          });

          trackCoverElement.src = trackCover;

          trackCoverElement.onerror = () => { trackCoverElement.classList.add('broken-cover'); };

          observer.unobserve(element);
        }
      });
    },
    {
      threshold: 0.1,
    },

  );


  const allIds = tracksSets ? Object.values(tracksSets).flat() : [];

  allIds?.forEach(id => {
    let trackButton = trackButtonTemplate(id);

    tracksList!.insertAdjacentHTML("beforeend", trackButton);

    const lastInsertedElement = tracksList!.lastElementChild as HTMLElement;

    if (lastInsertedElement) {
      observer.observe(lastInsertedElement)
    }
  });
}
