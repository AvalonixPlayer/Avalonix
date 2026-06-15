import { invoke } from "@tauri-apps/api/core";
import { Performer } from "../bindings/Performer";

let performerTemplate = (
  performer: Performer,
): string => `<div class="playable-sellect-item performer" >
  <h3 class="performer-title-title-button">${performer.title}</h3>
</div>`;

export async function fillPerformersList() {
  let performers = await invoke<Performer[]>("get_performers_datas").catch(() =>
    console.error("Error while getting tracks"),
  );

  if (performers == null) {
    return;
  }

  let performersList = document.getElementById("performers-list-section");

  performers.forEach((performer) => {
    performersList!.insertAdjacentHTML(
      "beforeend",
      performerTemplate(performer),
    );
  });
}
