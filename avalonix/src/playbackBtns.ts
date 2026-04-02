import { invoke } from "@tauri-apps/api/core";

export async function bindPlayback() {
  let pause = document.querySelector("#pause");

  pause!.addEventListener("click", async (_) => {
    await invoke("pause_or_continue");
    let pauseImg = pause?.querySelector("#pause-img") as HTMLImageElement;
    let continueImg = pause?.querySelector("#play-img") as HTMLImageElement;

    let on_pause = await invoke<boolean>("on_pause");
    pauseImg.style.display = on_pause == false ? "none" : "block";
    continueImg.style.display = on_pause == true ? "none" : "block";
  });

  let previous = document.querySelector("#previous");

  previous!.addEventListener("click", async (_) => {
    await invoke("previous_track");
  });

  let next = document.querySelector("#next");

  next!.addEventListener("click", async (_) => {
    await invoke("next_track");
  });
}
