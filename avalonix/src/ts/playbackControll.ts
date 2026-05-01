import { invoke } from "@tauri-apps/api/core";

export async function initPlaybackControll() {
  let previousTrackButton = document.querySelector("#previous-track-button");
  previousTrackButton!.addEventListener("click", async () => {
    await invoke("previous_track");
  });

  let nextTrackButton = document.querySelector("#next-track-button");
  nextTrackButton!.addEventListener("click", async () => {
    await invoke("next_track");
  });

  let pauseOrContinueTrackButton = document.querySelector("#pause-button");

  {
    let pause = await invoke("is_paused");

    console.log(pause);

    if (pause == true) {
      pauseOrContinueTrackButton!.querySelector("img")!.src =
        "src/assets/continue.png";
    } else {
      pauseOrContinueTrackButton!.querySelector("img")!.src =
        "src/assets/pause.png";
    }
  }

  pauseOrContinueTrackButton!.addEventListener("click", async () => {
    let pause = await invoke("pause_or_continue_track");

    if (pause == true) {
      pauseOrContinueTrackButton!.querySelector("img")!.src =
        "src/assets/pause.png";
    } else {
      pauseOrContinueTrackButton!.querySelector("img")!.src =
        "src/assets/continue.png";
    }
  });
}
