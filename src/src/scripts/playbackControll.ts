import { invoke } from "@tauri-apps/api/core";
import { listen } from "@tauri-apps/api/event";
import { Track } from "../bindings/Track";
import { PlayableResult } from "../bindings/PlayableResult";

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
  await fillPreviewTrack();

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
    await fillPreviewTrack();
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

async function fillPreviewTrack() {
  invoke<String>("get_current_track_uuid").then(async (uuid) => {
    let result = await invoke<PlayableResult>("get_playable_by_id", {
      mediaType: "Track",
      id: uuid,
    });

    let track = result!.data as Track | undefined;

    if (track) {
      let track_button = document.getElementById("current-track-show-button");
      track_button!.querySelector("h3")!.textContent = track.title;

      let title = (document.getElementById("track-title-show")!.textContent =
        track.title);

      let album = (document.getElementById("track-album-show")!.textContent =
        track.album);

      let performer = (document.getElementById(
        "track-performer-show",
      )!.textContent = track.performer);

      let coverItem = document
        .querySelector("#track-preview-cover")!
        .querySelector("img");

      invoke<string>("get_track_cover", { id: uuid })
        .then((cover) => {
          coverItem!.src = cover;
          if (cover == "") {
            coverItem!.src = "./src/assets/black.jpg";
          }
        })
        .catch((error) => {
          console.error(error);
          coverItem!.src = "./src/assets/black.jpg";
        });
    }
  });
}
