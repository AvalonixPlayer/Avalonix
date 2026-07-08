export function initResizeControll() {
  let currentTrackShowButton = document.getElementById(
    "current-track-show-button",
  );

  window.addEventListener("resize", () => {
    if (window.innerWidth < 700) {
      currentTrackShowButton!.style.display = "none";
    } else {
      currentTrackShowButton!.style.display = "flex";
    }
  });
}
