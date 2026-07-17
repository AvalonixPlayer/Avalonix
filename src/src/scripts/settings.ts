import { invoke } from "@tauri-apps/api/core";
import { DropList } from "./customElements";
import { pickDir, pickFile } from "./filePicker";
import { Theme } from "../bindings/Theme";

export async function initSettings() {
  let pathsList = document.querySelector("#lib-paths") as DropList;

  await theme();

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
}

async function theme() {
  let theme = await invoke<Theme>("get_theme");
  document.querySelector("#apply-settings")!.addEventListener("click", async () => await applyTheme());

  let textSize1 = document.querySelector("#font-size-input-1") as HTMLInputElement;
  let textSize2 = document.querySelector("#font-size-input-2") as HTMLInputElement;
  let textSize3 = document.querySelector("#font-size-input-3") as HTMLInputElement;
  let textSize4 = document.querySelector("#font-size-input-4") as HTMLInputElement;

  textSize1.value = theme.font_size1.toString();
  textSize2.value = theme.font_size2.toString();
  textSize3.value = theme.font_size3.toString();
  textSize4.value = theme.font_size4.toString();

  let bgColor = document.querySelector("#set-bg-color") as HTMLInputElement;

  bgColor.value = theme.background_color;

  let useBgImg = document.querySelector("#use-bg-img") as HTMLInputElement;

  useBgImg.checked = theme.use_background_image;

  await applyTheme();
}

async function applyTheme() {

  let textSize1 = document.querySelector("#font-size-input-1") as HTMLInputElement;
  let textSize2 = document.querySelector("#font-size-input-2") as HTMLInputElement;
  let textSize3 = document.querySelector("#font-size-input-3") as HTMLInputElement;
  let textSize4 = document.querySelector("#font-size-input-4") as HTMLInputElement;

  let bgColor = document.querySelector("#set-bg-color") as HTMLInputElement;

  let useBgImg = document.querySelector("#use-bg-img") as HTMLInputElement;

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

  const myTheme: Theme = {
    path_to_background_image: (await invoke<Theme>("get_theme")).path_to_background_image,
    background_color: bgColor.value,
    use_background_image: useBgImg.checked,
    font_size1: parseFontSize(textSize1),
    font_size2: parseFontSize(textSize2),
    font_size3: parseFontSize(textSize3),
    font_size4: parseFontSize(textSize4),
  };

  await invoke("set_theme", {theme: myTheme})

  await invoke("save_settings");

  const root = document.documentElement;

  let theme = await invoke<Theme>("get_theme");

  root.style.setProperty("--text-size-1", theme.font_size1.toString() + "px");
  root.style.setProperty("--text-size-2", theme.font_size2.toString() + "px");
  root.style.setProperty("--text-size-3", theme.font_size3.toString() + "px");
  root.style.setProperty("--text-size-4", theme.font_size4.toString() + "px");

  document.querySelector("#set-bg-image-button")!.addEventListener("click", async () => {
    let file = await pickFile();
    if (file) {
      theme.path_to_background_image = file;
      console.log(file);
      await invoke("set_theme", { theme: theme });
      await invoke("save_settings");
      let the1me = await invoke<Theme>("get_theme");
      console.log(the1me);
    }
  })

  document.body.style.background = theme.background_color.toString();
  if (theme.use_background_image)
  {
    invoke<string>("get_bg_gif_uri").then(x => {
      document.body.style.background = `url("${x}")`;
      document.body.style.backgroundSize = "cover";
      document.body.style.backgroundRepeat = "no-repeat";
      document.body.style.backgroundPosition = "center center";
      document.body.style.minHeight = "100vh";
      document.body.style.backgroundAttachment = "fixed";
    })
  }


}
