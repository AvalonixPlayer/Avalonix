import { invoke } from "@tauri-apps/api/core";
import { DropList } from "./customElements";
import { pickDir, pickFile } from "./filePicker";
import { Theme } from "../bindings/Theme";

export async function initSettings() {
  let pathsList = document.querySelector("#lib-paths") as DropList;

  await loadTheme();
  await applyTheme();

  let setBgImage = document.querySelector("#set-bg-image-button")! as HTMLElement;
  setBgImage.addEventListener("click", async () => {
    let file = await pickFile("img", ["gif", "png", "jpg", "jpeg"]);
    if (file) {
      setBgImage.dataset.bgPath = file;
    }
  });

  let applyBtn = document.querySelector("#apply-settings");
  applyBtn!.addEventListener("click", async () => {
    await applyTheme();
    await loadTheme();
  })

  document.querySelector("#add-path-to-lib")!.addEventListener("click", async () => {
    let dir = await pickDir();
    if (dir) {
      await invoke("add_path_to_lib", { path: dir });
      await updatePaths();
    }
  });

  await updatePaths();
  async function updatePaths() {
    let libraryPaths = await invoke<string[]>("get_library_paths");

    pathsList.querySelector(".drop-list-items")!.replaceChildren();

    libraryPaths.forEach(path => {
      let item = pathsList!.addItem(path);
      item.dataset.path = path;
      item.addEventListener("click", async () => {
        await invoke("remove_path_from_lib", {path: item.dataset.path});
        await updatePaths();
      })
    });
  }

  document.querySelector("#update-library")!.addEventListener("click", async () => {
    await invoke("update_library");
  })
}

async function loadTheme() {
  let theme = await invoke<Theme>("get_theme");

  let textSize1 = document.querySelector("#font-size-input-1") as HTMLInputElement;
  let textSize2 = document.querySelector("#font-size-input-2") as HTMLInputElement;
  let textSize3 = document.querySelector("#font-size-input-3") as HTMLInputElement;
  let textSize4 = document.querySelector("#font-size-input-4") as HTMLInputElement;

  textSize1.value = theme.font_size1.toString();
  textSize2.value = theme.font_size2.toString();
  textSize3.value = theme.font_size3.toString();
  textSize4.value = theme.font_size4.toString();

  let bgColor = document.querySelector("#set-bg-color") as HTMLInputElement;
  bgColor.value = theme.background_color.toString();

  let useBgImg = document.querySelector("#use-bg-img") as HTMLInputElement;
  useBgImg.checked = theme.use_background_image;

  let setBgImage = document.querySelector("#set-bg-image-button")! as HTMLElement;
  setBgImage.dataset.bgPath = theme.path_to_background_image?.toString();

  let bgBlurSlider = document.querySelector("#bg-blur-slider")! as HTMLInputElement;
  bgBlurSlider.value = theme.bg_blur.toString();

  let buttonHoverColor = document.querySelector("#set-button-hover-color")! as HTMLInputElement;
  buttonHoverColor.value = theme.button_hover_color;
  let buttonActiveColor = document.querySelector("#set-button-active-color")! as HTMLInputElement;
  buttonActiveColor.value = theme.button_active_color;
  let slidersColor = document.querySelector("#set-sliders-color")! as HTMLInputElement;
  slidersColor.value = theme.sliders_color;

  if (theme.use_background_image) {
    invoke<string>("get_bg_gif_uri").then(x => {
      let bgImg = document.querySelector("#background-image") as HTMLElement;
      bgImg.style.background = `url("${x}")`;
      bgImg.style.backgroundSize = "cover";
      bgImg.style.backgroundRepeat = "no-repeat";
      bgImg.style.backgroundPosition = "center center";
      bgImg.style.minHeight = "100vh";
      bgImg.style.backgroundAttachment = "fixed";
      root.style.setProperty("--background-blur", `${bgBlurSlider.value}px`);
    }).catch(err => {
      console.error(err);
      let bgImg = document.querySelector("#background-image") as HTMLElement;
      document.body.style.background = bgColor.value;
      bgImg.style.background = ``;
    })
  }
  else {
    document.body.style.background = bgColor.value;
    let bgImg = document.querySelector("#background-image") as HTMLElement;
    bgImg.style.background = ``;
  };

  const root = document.documentElement;

  root.style.setProperty("--text-size-1", theme.font_size1.toString() + "px");
  root.style.setProperty("--text-size-2", theme.font_size2.toString() + "px");
  root.style.setProperty("--text-size-3", theme.font_size3.toString() + "px");
  root.style.setProperty("--text-size-4", theme.font_size4.toString() + "px");

  root.style.setProperty("--button-hover-color", theme.button_hover_color.toString());
  root.style.setProperty("--button-active-color", theme.button_active_color.toString());
  root.style.setProperty("--sliders-color", theme.sliders_color.toString());
}

async function applyTheme() {
  let textSize1 = document.querySelector("#font-size-input-1") as HTMLInputElement;
  let textSize2 = document.querySelector("#font-size-input-2") as HTMLInputElement;
  let textSize3 = document.querySelector("#font-size-input-3") as HTMLInputElement;
  let textSize4 = document.querySelector("#font-size-input-4") as HTMLInputElement;

  let bgColor = document.querySelector("#set-bg-color") as HTMLInputElement;
  let useBgImg = document.querySelector("#use-bg-img") as HTMLInputElement;
  let setBgImage = document.querySelector("#set-bg-image-button")! as HTMLElement;
  let bgBlurSlider = document.querySelector("#bg-blur-slider")! as HTMLInputElement;

  let buttonHoverColor = document.querySelector("#set-button-hover-color")! as HTMLInputElement;
  let buttonActiveColor = document.querySelector("#set-button-active-color")! as HTMLInputElement;
  let slidersColor = document.querySelector("#set-sliders-color")! as HTMLInputElement;

  const parseFontSize = (input: HTMLInputElement | null): number => {
    let value = Number.parseFloat(input?.value || "");
    if (value < 5) {
      value = 5;
    }
    if (value > 70) {
      value = 70;
    }
    return Number.isNaN(value) ? 20 : value;
  };

  const newTheme: Theme = {
    path_to_background_image: setBgImage.dataset.bgPath!,
    background_color: bgColor.value,
    use_background_image: useBgImg.checked,
    button_hover_color: buttonHoverColor.value,
    button_active_color: buttonActiveColor.value,
    sliders_color: slidersColor.value,
    bg_blur: Number.parseInt(bgBlurSlider.value),
    font_size1: parseFontSize(textSize1),
    font_size2: parseFontSize(textSize2),
    font_size3: parseFontSize(textSize3),
    font_size4: parseFontSize(textSize4),
  };

  await invoke("set_theme", { theme: newTheme });
  await invoke("save_settings");
}
