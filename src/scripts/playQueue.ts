import { invoke } from "@tauri-apps/api/core";
import { MediaType } from "../bindings/MediaType";
import { listen } from "@tauri-apps/api/event";
import { PlayableResult } from "../bindings/PlayableResult";
import { Track } from "../bindings/Track";
import { updateSlider } from "./slider";
import { trackButtonTemplate } from "./tracksCreator";

export async function addMediaToQueue(mediaType: MediaType, mediaUuid: string) {
  await invoke("add_media_to_queue", { mediaType: mediaType, id: mediaUuid });
  await fillQueue();
}

export async function setMediaToQueue(mediaType: MediaType, mediaUuid: string) {
  await invoke("clear_queue");
  await invoke("add_media_to_queue", { mediaType: mediaType, id: mediaUuid });
  await fillQueue();
}

async function fillQueue() {
  let everyTracksIds = await invoke<string[]>("get_queue_tracks_ids");

  let tracksList = document.getElementById("queue-list");
  tracksList!.innerHTML = "";

  const observer = new IntersectionObserver(
    (enteries, observer) => {
      enteries.forEach(async (entry) => {
        if (entry.isIntersecting) {
          const element = entry.target as HTMLElement;
          let uuid = element.getAttribute("data-uuid");
          let track = (
            await invoke<PlayableResult>("get_playable_by_id", {
              mediaType: "Track",
              id: uuid,
            })
          ).data as Track;

          let trackTitle = element.querySelector(".track-title")!;
          trackTitle.textContent = track.title;
          let trackPerformer = element.querySelector(".track-performer")!;
          trackPerformer.textContent = track.performer;
          let trackGenre = element.querySelector(".track-genre")!;
          trackGenre.textContent = track.genre;

          element.addEventListener("click", async () => {
            await invoke("start_track_in_queue_by_id", {id: uuid})
          });

          let trackCover: string = await invoke("get_track_cover", { id: uuid });

          let trackCoverElement = element.querySelector("img") as HTMLImageElement;

          trackCoverElement.animate([
            { opacity: 0, transform: 'translateX(-20px)'},
            { opacity: 1, transform: 'translateX(0)'}
          ], {
            duration: 1400,
            easing: 'ease',
            fill: 'forwards'
          });

          trackCoverElement.src = trackCover;

          trackCoverElement.onerror = () => { trackCoverElement.classList.add('broken-cover'); };

          observer.unobserve(element);
        }
      });
    },
    {
      threshold: 0.1,
    },

  );


  const allIds = everyTracksIds ? Object.values(everyTracksIds).flat() : [];

  allIds?.forEach(id => {
    let trackButton = trackButtonTemplate(id);

    tracksList!.insertAdjacentHTML("beforeend", trackButton);

    const lastInsertedElement = tracksList!.lastElementChild as HTMLElement;

    if (lastInsertedElement) {
      observer.observe(lastInsertedElement)
    }
  });
}

export async function initPlayqueueControll() {
  await updatePlayqueueTrackPreview();
  await fillQueue();
  await initControllTrackTime();
  await initControllQueueButtons();

  let pauseBtn = document.getElementById("pause-track-button")!;
  await listen("cur-track-changed", async () =>
  {
      if ((await invoke<boolean>("get_media_player_state")) == false)
        pauseBtn.querySelector("img")!.src = "/pause.png";
      else {
        pauseBtn.querySelector("img")!.src = "/play.png";
      }
      await updatePlayqueueTrackPreview();
    });
  await listen("queue-updated", async () => {
    await fillQueue();
  })
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
