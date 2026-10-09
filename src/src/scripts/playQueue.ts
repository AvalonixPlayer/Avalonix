import { invoke } from "@tauri-apps/api/core";
import { MediaType } from "../bindings/MediaType";
import { listen } from "@tauri-apps/api/event";
import { PlayableResult } from "../bindings/PlayableResult";
import { Track } from "../bindings/Track";
import { updateSlider } from "./slider";

export async function initPlayqueue() {

}

export async function addMediaToQueue(mediaType: MediaType, mediaUuid: string) {
  await invoke("add_media_to_queue", {mediaType: mediaType, id: mediaUuid})
}

export async function initPlayqueueControll() {
  await updatePlayqueueTrackPreview();
  await initControllTrackTime();
  await initControllQueueButtons();
  await listen("cur-track-changed", async () => {
    await updatePlayqueueTrackPreview();
  });
}

async function initControllQueueButtons() {
  document.getElementById("previous-track-button")?.addEventListener("click", async () => {
    await invoke("previous_track");
  })

  document.getElementById("next-track-button")?.addEventListener("click", async () => {
    await invoke("next_track");
  })

  await process();

  document.getElementById("pause-track-button")?.addEventListener("click", async () => {
    await process();
  })

  async function process() {
    await invoke("switch_media_player_state");

    let paused = await invoke<boolean>("get_media_player_state");

    if (paused) {
      (document.getElementById("pause-track-button")?.querySelector("img") as HTMLImageElement).src = "/play.png";
    }
    else {
      (document.getElementById("pause-track-button")?.querySelector("img") as HTMLImageElement).src = "/pause.png";
    }
  }
}

async function initControllTrackTime() {
  let slider = document.getElementById("track-time-slider") as HTMLInputElement;

  let playedTime = document.getElementById("played-time") as Element;
  let leftTime = document.getElementById("left-time") as Element;

  let userSelecting = false;

  slider.addEventListener("pointerdown", () => { userSelecting = true });
  slider.addEventListener("pointerup", () => { userSelecting = false });
  setInterval(async () => {
    let maxValue = await invoke<number>("get_media_player_end_pos");
    if (userSelecting) {
      await invoke("try_seek", {pos: Number.parseInt(slider.value)});
    }
    else {
      let value = await invoke<number>("get_current_media_player_pos");
      slider.max = maxValue.toString();
      slider.value = value.toString();
      updateSlider(slider);
    }
    playedTime.textContent = toMMSS(Number.parseInt(slider.value));
    leftTime.textContent = toMMSS(Number.parseInt(slider.max) - Number.parseInt(slider.value));

  }, 100)



  function toMMSS(secs: number) {
    var minutes = Math.floor(secs / 60).toString();
    var seconds = (secs - (Math.floor(secs / 60) * 60)).toString();

    if (minutes.length < 2) minutes = "0" + minutes;
    if (seconds.length < 2) seconds = "0" + seconds;
    return minutes+':'+seconds;
  }
}

async function updatePlayqueueTrackPreview() {
  try {
    let currentTrackUuid = await invoke<string>("get_current_track_uuid");

    if (currentTrackUuid == null) return;

    let track = (
      await invoke<PlayableResult>("get_playable_by_id", {
        mediaType: "Track",
        id: currentTrackUuid,
      })
    ).data as Track;

    let trackPreviewInControll = document.getElementById("track-preview-in-queue-control");

    let trackPreviewInControllName = trackPreviewInControll?.querySelector("h4") as Element;
    let trackPreviewInControllPerformer = trackPreviewInControll?.querySelector("h5") as Element;
    let trackPreviewInControllCover = trackPreviewInControll?.querySelector("img") as HTMLImageElement;

    let trackCover: string = await invoke("get_track_cover", { id: currentTrackUuid });

    trackPreviewInControllName.textContent = track.title;
    trackPreviewInControllPerformer.textContent = track.performer;

    trackPreviewInControllCover.src = trackCover;

    trackPreviewInControllCover.animate([
      { opacity: 0, transform: 'translateX(-20px)'},
      { opacity: 1, transform: 'translateX(0)'}
    ], {
      duration: 1400,
      easing: 'ease',
      fill: 'forwards'
    });
  }
catch {}
}
