import { invoke } from "@tauri-apps/api/core";
import { open } from "@tauri-apps/plugin-dialog";

const saveSettingsBtn = document.getElementById("save-settings");
const addPathToLibBtn = document.getElementById("add-path-to-music-btn");

export async function initSettings() {
  saveSettingsBtn?.addEventListener("click", async (_) => {
    await invoke("save_settings");
  });

  addPathToLibBtn?.addEventListener("click", async (_) => {
    let dirs = await OpenDirPicker();
    if (dirs) {
      await invoke("add_music_folder", { paths: dirs });
    }
  });
}

async function OpenDirPicker() {
  const dirs = await open({
    multiple: true,
    directory: true,
  });

  return dirs;
}
