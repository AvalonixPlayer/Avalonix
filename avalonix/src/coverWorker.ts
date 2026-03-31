export function createCoverUrl(coverDataURI: string | null) {
  if (coverDataURI != null) {
    return coverDataURI;
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
