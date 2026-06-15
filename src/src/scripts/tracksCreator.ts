import { invoke } from "@tauri-apps/api/core";
import { Track } from "../bindings/Track";

const trackTemplate = (track: Track): string => `
  <div class="playable-sellect-item track" >
    <h3 class="track-title-button">${track.title}</h3>
    <h3 class="track-performer-button">${track.performer}</h3>
    <h3 class="track-album-title-button"> ${track.album} </h3>
  </div>`;

export async function fillTracksList() {
  let tracks = await invoke<Track[]>("get_tracks_datas").catch(() =>
    console.error("Error while getting tracks"),
  );
  if (tracks == null) {
    return;
  }
  let tracksList = document.getElementById("tracks-list-section");
  tracks.forEach((track) => {
    tracksList!.insertAdjacentHTML("beforeend", trackTemplate(track));
  });
}
