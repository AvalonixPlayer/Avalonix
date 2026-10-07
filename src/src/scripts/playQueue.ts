import { invoke } from "@tauri-apps/api/core";
import { MediaType } from "../bindings/MediaType";
import { listen } from "@tauri-apps/api/event";
import { PlayableResult } from "../bindings/PlayableResult";
import { Track } from "../bindings/Track";

export async function initPlayqueue() {

}

export async function addMediaToQueue(mediaType: MediaType, mediaUuid: string) {
  await invoke("add_media_to_queue", {mediaType: mediaType, id: mediaUuid})
}

export async function initPlayqueueControll() {
  await updatePlayqueueTrackPreview();
  await listen("cur-track-changed", async () => {
    await updatePlayqueueTrackPreview();
  });
}

async function updatePlayqueueTrackPreview() {
  let trackPreviewInControll = document.getElementById("track-preview-in-queue-control");

  let trackPreviewInControllName = trackPreviewInControll?.querySelector("h4") as Element;
  let trackPreviewInControllPerformer = trackPreviewInControll?.querySelector("h5") as Element;
  let trackPreviewInControllCover = trackPreviewInControll?.querySelector("img") as HTMLImageElement;

  let currentTrackUuid = await invoke<string>("get_current_track_uuid");

  let track = (
    await invoke<PlayableResult>("get_playable_by_id", {
      mediaType: "Track",
      id: currentTrackUuid,
    })
  ).data as Track;
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
