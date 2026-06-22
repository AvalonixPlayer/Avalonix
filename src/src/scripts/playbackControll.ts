import { invoke } from "@tauri-apps/api/core";
import { listen } from "@tauri-apps/api/event";

export async function initPlaybackControll() {
  let pauseBtn = document.querySelector("#pause")!;

  async function ch() {
    if ((await invoke<boolean>("get_media_player_state")) == false)
      pauseBtn.querySelector("img")!.src = "/pause.png";
    else {
      pauseBtn.querySelector("img")!.src = "/play.png";
    }
  }

  await ch();

  pauseBtn.addEventListener("click", async () => {
    await invoke("switch_media_player_state");
    await ch();
  });

  let timeSlider = document.querySelector(
    "#play-time-controll-slider",
  )! as HTMLInputElement;

  let cur_time = document.querySelector("#track-cur-time")! as HTMLElement;
  let left_time = document.querySelector("#track-left-time")! as HTMLElement;
  let seeking = false;

  setInterval(async () => {
    let pos = await invoke<number>("get_current_media_player_pos");
    let end = await invoke<number>("get_media_player_end_pos");
    if (!seeking) {
      timeSlider.value = pos.toString();
      cur_time.textContent = formatTime(pos);
      left_time.textContent = formatTime(end - pos);
    } else {
      cur_time.textContent = formatTime(Number.parseInt(timeSlider.value));
      left_time.textContent = formatTime(
        end - Number.parseInt(timeSlider.value),
      );
    }
    timeSlider.max = end.toString();
  }, 100);

  timeSlider.addEventListener("pointerdown", async () => {
    seeking = true;
  });

  timeSlider.addEventListener("pointerup", async () => {
    seeking = false;
    await invoke<number>("try_seek", {
      pos: Number.parseInt(timeSlider.value),
    });
  });

  listen("cur-track-changed", async () => {
    timeSlider.max = (
      await invoke<number>("get_media_player_end_pos")
    ).toString();
    timeSlider.value = "0";
    ch();
  });
}

function formatTime(totalSeconds: number): string {
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;

  const formattedMinutes = String(minutes).padStart(2, "0");
  const formattedSeconds = String(seconds).padStart(2, "0");

  if (hours > 0) {
    const formattedHours = String(hours).padStart(2, "0");
    return `${formattedHours}:${formattedMinutes}:${formattedSeconds}`;
  }

  return `${formattedMinutes}:${formattedSeconds}`;
}
