import { invoke } from "@tauri-apps/api/core";
import { Performer } from "../bindings/Performer";
import { PlayableResult } from "../bindings/PlayableResult";
import { addMediaToQueue } from "./playQueue";

let performerTemplate = (performer_uuid: string): string =>
  `<div class="playable-sellect-item performer" data-uuid="${performer_uuid}">
  <h3 class="performer-title-button"></h3>
</div>`;

export async function fillPerformersList() {
  let performers_ids = await invoke<string[]>("get_playables_ids", {
    mediaType: "Performer",
  }).catch(() => console.error("Error while getting performers ids"));

  if (performers_ids == null) {
    return;
  }

  let performersList = document.getElementById("performers-list-section");
  performersList!.innerHTML = "";

  const observer = new IntersectionObserver(
    (enteries, observer) => {
      enteries!.forEach(async (entry) => {
        if (entry.isIntersecting) {
          const element = entry.target as HTMLElement;
          let uuid = element.getAttribute("data-uuid");
          let performer = (
            await invoke<PlayableResult>("get_playable_by_id", {
              mediaType: "Performer",
              id: uuid,
            })
          ).data as Performer;

          let performerTitleButton = element.querySelector(
            ".performer-title-button",
          )!;
          performerTitleButton.textContent = performer.title;
          performerTitleButton.addEventListener("click", async () => {
            addMediaToQueue("Performer", uuid!);
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

  performers_ids.forEach((performer_id) => {
    let element = performerTemplate(performer_id);
    performersList!.insertAdjacentHTML("beforeend", element);

    const lastInsertedElement = performersList!.lastElementChild as HTMLElement;

    if (lastInsertedElement) {
      observer.observe(lastInsertedElement);
    }
  });
}
