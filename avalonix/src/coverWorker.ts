export async function createCoverUrlAsync(coverData: Uint8Array | null) {
  if (coverData != null) {
    const byteArray = new Uint8Array(coverData);
    var blob = new Blob([byteArray], { type: "image/jpg" });
    return URL.createObjectURL(blob);
  } else {
    const canvas = document.createElement("canvas");
    canvas.width = 1;
    canvas.height = 1;
    const ctx = canvas.getContext("2d");

    ctx!.fillStyle = "black";
    ctx!.fillRect(0, 0, 1, 1);
    return canvas.toDataURL("image/jpg");
  }
}
