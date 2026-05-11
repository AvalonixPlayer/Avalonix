import { invoke } from "@tauri-apps/api/core";
import { listen } from "@tauri-apps/api/event";
import { updateQueue } from "./queueFiller";

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

  let volumeSlider = document.querySelector(
    "#volume-controll-range",
  ) as HTMLInputElement;
  let volume = (await invoke<number>("get_current_volume")) * 100;
  volumeSlider.value = volume.toString();

  volumeSlider!.addEventListener("input", async () => {
    await invoke("set_current_volume", {
      volume: parseFloat(volumeSlider.value) / 100,
    });
  });

  let shuffleButton = document.querySelector("#shuffle-button");
  await check();

  shuffleButton!.addEventListener("click", async () => {
    await invoke<boolean>("shuffle_or_unshuffle");
    await check();
    await updateQueue();
  });

  listen("track-updated", async () => {
    await check();
  });

  async function check() {
    let pause = await invoke("is_paused");

    if (pause == true) {
      pauseOrContinueTrackButton!.querySelector("img")!.src = "/continue.png";
    } else {
      pauseOrContinueTrackButton!.querySelector("img")!.src = "/pause.png";
    }

    let shuffle = await invoke<boolean>("shuffle_state");

    if (shuffle) {
      shuffleButton!.querySelector("img")!.src = "./shuffle.png";
    } else {
      shuffleButton!.querySelector("img")!.src = "./unshuffle.png";
    }
  }

  pauseOrContinueTrackButton!.addEventListener("click", async () => {
    let pause = await invoke("pause_or_continue_track");

    if (pause == true) {
      pauseOrContinueTrackButton!.querySelector("img")!.src = "/pause.png";
    } else {
      pauseOrContinueTrackButton!.querySelector("img")!.src = "/continue.png";
    }
  });

  const trackPositionSlider = document.getElementById(
    "play-queue-time-controll-range",
  ) as HTMLInputElement;

  let userSeek = false;

  setInterval(async () => {
    if (userSeek) return;
    let trackPos = (await invoke("get_track_position")) as number;
    let trackLen = (await invoke("get_track_durration")) as number;
    updateTimeTexts(trackPos, trackLen - trackPos);

    trackPositionSlider.value = ((trackPos / trackLen) * 100).toString();
  }, 100);

  let debounceTimer: ReturnType<typeof setTimeout> | undefined;

  trackPositionSlider.addEventListener("input", async () => {
    userSeek = true;
    let trackPos = (await invoke("get_track_position")) as number;
    let trackLen = (await invoke("get_track_durration")) as number;

    console.log(trackPos);
    console.log(trackLen);

    updateTimeTexts(trackPos, trackLen - trackPos);
    clearTimeout(debounceTimer);

    debounceTimer = setTimeout(async () => {
      let float_secs = (trackLen / 100) * parseInt(trackPositionSlider.value);

      let second = Math.floor(float_secs);

      await invoke("seek", {
        seekSecond: second,
      });
      userSeek = false;
    }, 100);
  });

  function updateTimeTexts(curTime: number, leftTime: number) {
    let cur = document.getElementById("cur-track-time");
    let left = document.getElementById("left-track-time");
    cur!.textContent = new Date(curTime * 1000)
      .toISOString()
      .substring(11, 19)
      .toString();
    left!.textContent = new Date(leftTime * 1000)
      .toISOString()
      .substring(11, 19)
      .toString();
  }
}
