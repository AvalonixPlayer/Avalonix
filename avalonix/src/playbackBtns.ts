import { invoke } from "@tauri-apps/api/core";

export async function bindPlayback() {
    let pause = document.querySelector("#pause");

    pause!.addEventListener("click", async (_) => {
        await invoke("pause_or_continue");
    })

    let previous = document.querySelector("#previous");

    previous!.addEventListener("click", async (_) => {
        await invoke("previous_track");
    })

    let next = document.querySelector("#next");

    next!.addEventListener("click", async (_) => {
        await invoke("next_track");
    })
}