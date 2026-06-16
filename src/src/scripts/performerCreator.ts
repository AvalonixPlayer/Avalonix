import { invoke } from "@tauri-apps/api/core";
import { Performer } from "../bindings/Performer";

let performerTemplate = (performer_uuid: string): string =>
  `<div class="playable-sellect-item performer" data-uuid="${performer_uuid}">
  <h3 class="performer-title-button"></h3>
</div>`;

export async function fillPerformersList() {
  let performers_ids = await invoke<string[]>("get_performers_ids").catch(() =>
    console.error("Error while getting performers ids"),
  );

  if (performers_ids == null) {
    return;
  }

  let performersList = document.getElementById("performers-list-section");

  const observer = new IntersectionObserver(
    (enteries, observer) => {
      enteries.forEach(async (entry) => {
        if (entry.isIntersecting) {
          const element = entry.target as HTMLElement;
          let uuid = element.getAttribute("data-uuid");
          let performer = await invoke<Performer>("get_performer_by_id", {
            id: uuid,
          });

          console.log(uuid);

          element.querySelector(".performer-title-button")!.textContent =
            performer.title;
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
