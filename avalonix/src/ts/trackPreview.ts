import { event } from "@tauri-apps/api";
import { invoke } from "@tauri-apps/api/core";
import { listen } from "@tauri-apps/api/event";
import { TrackMetadata } from "../bindings/TrackMetadata";

export async function initTrackPreview() {
  listen("track-updated", async () => {
    let metadata = await invoke<TrackMetadata>("get_cur_track_metadata");
    let cover = await invoke<string>("get_track_cover");
    updateTrackPreview(metadata, cover);
  });
}

export async function updateTrackPreview(
  metadata: TrackMetadata | null,
  cover: string | null,
) {
  if (metadata != null) {
    let trackPreview = document.querySelector("#track-preview") as HTMLElement;
    trackPreview.querySelector("h1")!.textContent = metadata.title;
    trackPreview.querySelector("h2")!.textContent = metadata.album;
    trackPreview.querySelector("h3")!.textContent = metadata.artist;
    if (cover == "".toString()) {
      cover = "./src/assets/black.jpg";
    }
    trackPreview.querySelector("img")!.src = cover!;
  } else {
    let metadata;
    await invoke<TrackMetadata>("get_cur_track_metadata")
      .then(async (x) => {
        console.log(2);

        let cover = await invoke<string>("get_track_cover");
        metadata = x;

        await updateTrackPreview(metadata, cover);
      })
      .catch((err) => {
        let cover = "./src/assets/black.jpg";
        let trackPreview = document.querySelector(
          "#track-preview",
        ) as HTMLElement;
        trackPreview.querySelector("img")!.src = cover;
        console.debug(err);
      });
  }
}
