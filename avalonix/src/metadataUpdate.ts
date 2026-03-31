import { Metadata } from "./bindings/Metadata";
import { createCoverUrl } from "./coverWorker";

export async function UpdateTrackUI(metadata: Metadata | null) {
  let coverData = metadata?.track_cover_uri;

  setOtherMetadata(metadata);
  setCover(coverData != null ? coverData : null);
}

function setOtherMetadata(metadata: Metadata | null) {
  var title = document.getElementById("track-name") as HTMLTextAreaElement;
  var album = document.getElementById("album-name") as HTMLTextAreaElement;
  title.textContent = metadata?.title || "No title";
  album.textContent = metadata?.album || "No album";
}

function setCover(track_cover_uri: string | null) {
  var img = document.getElementById("track-cover") as HTMLImageElement;

  img.src = createCoverUrl(track_cover_uri);
}
