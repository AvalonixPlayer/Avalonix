import { invoke } from "@tauri-apps/api/core";

export async function updateLibrary() {

  await invoke("update_library");
}
