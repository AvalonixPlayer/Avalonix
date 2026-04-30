import { invoke } from "@tauri-apps/api/core";
import { tracksFilerDatas } from "./mainFiller";

const queueList = document.getElementById("queue-list") as HTMLElement;
const buttonTemplate = document.getElementById(
  "track-sellect-in-queue-button-template",
) as HTMLTemplateElement;

export async function updateQueue() {
  let indexes = await invoke<Array<number>>("get_tracks_in_queue_indexes");

  queueList.innerHTML = "";
  indexes.forEach((index) => {
    let button = buttonTemplate.content.cloneNode(true) as DocumentFragment;

    let sellect = button.getElementById("start-track-from-queue-button");

    sellect!.addEventListener("click", async () => {
      await invoke("start_track", { index: index });
    });

    let data = tracksFilerDatas[index];

    let title = button.querySelector("h3");
    title!.textContent = data.title;

    queueList.append(button);
  });
}
