import { invoke } from "@tauri-apps/api/core";
import { addMediaToQueue, setMediaToQueue } from "./playQueue";
import { PlayableResult } from "../bindings/PlayableResult";
import { Playlist } from "../bindings/Playlist";

export class ContextMenu {
  private x: number = 0;
  private y: number = 0;

  private html: string = `<div class="context-menu"></div>`;

  private fieldTemp = (name: string): string =>
  `<div class="context-menu-field">
      <h3>${name}</h3>
  </div>`;

  constructor(x: number, y: number) {
    this.x = x;
    this.y = y;

    this.draw();
  }

  private closeMenu() {
    document.body.querySelector(".context-menu")?.remove();
  }

  private async addField(name: string, callback: (...args: any[]) => Promise<void>) {
    const menu = document.body.querySelector(".context-menu") as HTMLElement;
    menu.insertAdjacentHTML("beforeend", this.fieldTemp(name));
    menu.lastChild!.addEventListener("click", async () => {
      await callback();
    });
  }

  private async removeFields() {
    const menu = document.body.querySelector(".context-menu");
    menu!.innerHTML! = "";
  }

  private draw() {
    this.closeMenu();
    document.body.insertAdjacentHTML("beforeend", this.html);
    const newMenu = (document.querySelector(".context-menu") as HTMLElement);
    document.getElementById("panels-grid")!.addEventListener("click", () => {
      this.closeMenu();
    });
    newMenu.style.left = `${this.x}px`;
    newMenu.style.top = `${this.y}px`;
  }

  async spawnForTrackInLibrary(uuid: string) {
    this.addField("Add to queue", async () => {
      await addMediaToQueue("Track", uuid);
      this.closeMenu();
    });
    this.addField("Set queue", async () => {
      await setMediaToQueue("Track", uuid);
      this.closeMenu();
    });
    this.addField("Add to playlist", async () => {
      this.removeFields();
      await this.spawnForSellectPlaylist(uuid);
    });
  }

  async spawnForSellectPlaylist(uuid: String) {
    let playlistsSet = await invoke<Record<string, string[]>>("get_playables_ids", {
      mediaType: "Playlist",
      sortBy: {SortPerformers: "ByName"}
    }).catch(() => console.error("Error while getting playlists in library ids"));
    const allIds = playlistsSet ? Object.values(playlistsSet).flat() : [];

    allIds.forEach(async (id) => {
      let playlist = (
        await invoke<PlayableResult>("get_playable_by_id", {
          mediaType: "Playlist",
          id: id,
        })
      ).data as Playlist;
      this.addField(playlist.title, async () => {
        await invoke("add_track_to_playlist", {
          playlistUuid: id,
          trackUuid: uuid
        });
        this.closeMenu();
      });
    })
  }
}
