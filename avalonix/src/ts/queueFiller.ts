import { invoke } from "@tauri-apps/api/core";
import { tracksFilerDatas } from "./mainFiller";

const queueList = document.getElementById("queue-list") as HTMLElement;
const buttonTemplate = document.getElementById(
  "track-sellect-in-queue-button-template",
) as HTMLTemplateElement;

export async function updateQueue() {
  let get_tracks_in_queue_ids = await invoke<Array<Array<number>>>(
    "get_tracks_in_queue_ids",
  );

  queueList.innerHTML = "";
  get_tracks_in_queue_ids.forEach((id) => {
    let button = buttonTemplate.content.cloneNode(true) as DocumentFragment;

    let sellect = button.getElementById("start-track-from-queue-button");
    let remove = button.getElementById("remove-track-from-queue");

    sellect!.addEventListener("click", async () => {
      await invoke("start_track", { id: id });
    });
    remove!.addEventListener("click", async () => {
      //await invoke("remove_track_from_queue", { index: index });
    });

    let data = tracksFilerDatas.find(
      (data) => JSON.stringify(id) == JSON.stringify(data.id),
    );

    let title = button.querySelector("h3");
    title!.textContent = data!.title;

    queueList.append(button);
  });
}
