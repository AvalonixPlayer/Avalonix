import { invoke } from "@tauri-apps/api/core";

export async function initPlayback() {
  await trackIndexControll();
}

async function trackIndexControll() {
  let previous = document.querySelector("#previous-track");
  let next = document.querySelector("#next-track");

  previous!.addEventListener("click", async () => {
    await invoke("previous_track");
  });

  next!.addEventListener("click", async () => {
    await invoke("next_track");
  });
}
