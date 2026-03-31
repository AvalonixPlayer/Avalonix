import { Metadata } from "./bindings/Metadata";
import { createCoverUrlAsync } from "./coverWorker";

export async function UpdateTrackUI(metadata: Metadata | null) {
  let coverData = metadata?.track_cover;

  setOtherMetadata(metadata);
  setCoverAsync(coverData != null ? coverData : null);
}

function setOtherMetadata(metadata: Metadata | null) {
  var title = document.getElementById("track-name") as HTMLTextAreaElement;
  var album = document.getElementById("album-name") as HTMLTextAreaElement;
  title.textContent = metadata?.title || "No title";
  album.textContent = metadata?.album || "No album";
}

async function setCoverAsync(track_cover: Uint8Array | null) {
  await createCoverUrlAsync(track_cover).then((imgURL) => {
    var img = document.getElementById("track-cover") as HTMLImageElement;
    img.src = imgURL;
  });
}
