export async function resize() {
  const queueControlGrid = document.getElementById("queue-control-grid") as HTMLElement;
  window.addEventListener("resize", async () => {
    let x = window.innerWidth;
    let y = window.innerHeight;
    console.log(x);
    if (x >= 800) queueControlGrid.style.setProperty("--active-secondary-in-queue-controll", "1");
      else queueControlGrid.style.setProperty("--active-secondary-in-queue-controll", "0");

  })
}
