import { invoke } from "@tauri-apps/api/core";
import { DropList } from "./customElements";
import { pickDir } from "./filPicker";

export async function initSettings() {
  let pathsList = document.querySelector("#lib-paths") as DropList;

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
