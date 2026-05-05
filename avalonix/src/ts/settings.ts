import { invoke } from "@tauri-apps/api/core";
import { open } from "@tauri-apps/plugin-dialog";
import { getLibFromDB } from "./main";

const removePathFromLibButtonTemplate = document.getElementById(
  "remove-path-from-lib-button-template",
) as HTMLTemplateElement;

let pathDropIsActive = false;

export async function initSettings() {
  await initLibraryButtons();
}

async function initLibraryButtons() {
  let addLibraryFolderDrop = document.getElementById("add-folder-to-lib-drop");
  addLibraryFolderDrop!.addEventListener("click", async () => {
    pathDropIsActive = !pathDropIsActive;

    let list = document.getElementById("settings-lib-paths-list");

    if (pathDropIsActive) {
      let paths = await invoke<Array<string>>(
        "get_library_folders_from_settings",
      );

      paths.forEach((path, index) => {
        let clone = removePathFromLibButtonTemplate.content.cloneNode(
          true,
        ) as HTMLElement;

        let pathElement = clone.querySelector("h2");
        pathElement!.textContent = path;

        let removeBtn = clone.querySelector("#remove-path-from-lib-button");
        removeBtn?.addEventListener("click", async () => {
          await invoke("remove_folder_path_from_library", { path: path });
          console.log("remove " + index);
          list?.removeChild(list.children[index]);
        });
        list?.append(clone);
      });
    } else {
      list!.innerHTML = "";
    }
  });

  let addFolderToLib = document.getElementById("add-path-to-lib-button");
  addFolderToLib!.addEventListener("click", async () => {
    const folder = await open({
      directory: true,
      multiple: false,
    });

    if (folder != null) {
      await invoke("add_folder_path_to_library", { path: folder });
    }
  });

  let updateLibraryBtn = document.getElementById("settings-update-library");
  updateLibraryBtn!.addEventListener("click", async () => {
    console.log("update");
    await invoke("update_library");
    await getLibFromDB();
    console.log("updated");
  });

  let clearLibraryBtn = document.getElementById("settings-clear-library");
  clearLibraryBtn!.addEventListener("click", async () => {
    invoke("clear_library")
      .then(() => console.log("cleared"))
      .catch((err) => console.log(err));
    await getLibFromDB();
  });
}
