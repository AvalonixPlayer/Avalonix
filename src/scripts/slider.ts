export async function initSliders() {
  let sliders = document.querySelectorAll('input[type=range]');
  sliders.forEach(async (e) => {
    let slider = e as HTMLInputElement;
    updateSlider(slider);
    e.addEventListener("input", async () => {
      updateSlider(slider);
    })
  });
}

export function updateSlider(slider: HTMLInputElement) {
  slider.style.setProperty("--value", `${Number.parseInt(slider.value) / Number.parseInt(slider.max) * 100}%`);
}
