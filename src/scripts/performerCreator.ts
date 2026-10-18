import { invoke } from "@tauri-apps/api/core";
import { Performer } from "../bindings/Performer";
import { PlayableResult } from "../bindings/PlayableResult";
import { addMediaToQueue } from "./playQueue";

const performerButtonTemplate = (performer_uuid: string): string =>
  `<div class="button playable performer-button" data-uuid="${performer_uuid}">
    <div>
      <h4 class="performer-title"></h3>
    </div>
    <div>
      <h4 class="performer-lenght"></h5>
    </div>
  </div>`;

export async function fillPerformersLibrary() {
  let sortModPicker = document.getElementById("performers-sort-select") as HTMLSelectElement;

  let sortBy = "ByName";
  sortModPicker.addEventListener("change", async (e) => {
    sortBy = (e.target as HTMLSelectElement).value;
    await spawn(sortBy);
  });
  await spawn(sortBy);
}

async function spawn(sortBy: string) {
  let performersSets = await invoke<Record<string, string[]>>("get_playables_ids", {
    mediaType: "Performer",
    sortBy: {SortPerformers: sortBy}
  }).catch(() => console.error("Error while getting performers in library ids"));

  let performers = document.getElementById("performers-list");
  performers!.innerHTML = "";

  const observer = new IntersectionObserver(
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

          let performerTitle = element.querySelector(".performer-title")!;
          performerTitle.textContent = performer.title;
          let len = element.querySelector(".performer-lenght")!;
          len.textContent = performer.tracks_ids.length.toString();

          element.addEventListener("click", async () => {
            await addMediaToQueue("Performer", uuid!);
          });

          observer.unobserve(element);
        }
      });
    },
    {
      threshold: 0.1,
    },

  );


  const allIds = performersSets ? Object.values(performersSets).flat() : [];

  allIds?.forEach(id => {
    let trackButton = performerButtonTemplate(id);

    performers!.insertAdjacentHTML("beforeend", trackButton);

    const lastInsertedElement = performers!.lastElementChild as HTMLElement;

    if (lastInsertedElement) {
      observer.observe(lastInsertedElement)
    }
  });
}
