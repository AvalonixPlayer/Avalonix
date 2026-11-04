import { invoke } from "@tauri-apps/api/core";
import { addMediaToQueue, removeTrack, setMediaToQueue, startTrack } from "./playQueue";
import { PlayableResult } from "../bindings/PlayableResult";
import { Playlist } from "../bindings/Playlist";
import { MediaType } from "../bindings/MediaType";

export class ContextMenu {
  private x: number = 0;
  private y: number = 0;

  private html: string = `<div class="context-menu" id="alive-context"></div>`;

  private fieldTemp = (name: string): string =>
  `<div class="context-menu-field">
      <h4>${name}</h4>
  </div>`;

  constructor(x: number, y: number) {
    this.x = x;
    this.y = y;

    this.draw();
  }

  private closeMenu() {
    let menu = document.getElementById("alive-context");
    if (menu == null) return;
    menu!.id = 'dead-menu';
    menu!.animate([
      { opacity: 1, transform: "scaleY(1)"},
      { opacity: 0, transform: "scaleY(0)"}
    ], {
      duration: 500,
      easing: 'ease',
      fill: 'forwards'
    });

    setTimeout(() => {menu!.remove(); }, 500);

  }

  private async addField(name: string, callback: (...args: any[]) => Promise<void>) {
    const menu = document.getElementById("alive-context") as HTMLElement;
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
    const newMenu = (document.getElementById("alive-context") as HTMLElement);
    document.getElementById("panels-grid")!.addEventListener("click", () => {
      this.closeMenu();
    });
    newMenu.style.left = `${this.x}px`;
    newMenu.style.top = `${this.y}px`;

    this.animate();
  }

  private animate() {
    const newMenu = (document.getElementById("alive-context") as HTMLElement);
    newMenu.animate([
      { opacity: 0, transform: "scaleY(0)"},
      { opacity: 1, transform: "scaleY(1)"}
    ], {
      duration: 500,
      easing: 'ease',
      fill: 'forwards'
    });
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
      await this.spawnForSellectPlaylist(uuid, "Track");
    });
  }

  async spawnForAlbumInLibrary(uuid: string) {
    this.addField("Add to queue", async () => {
      await addMediaToQueue("Album", uuid);
      this.closeMenu();
    });
    this.addField("Set queue", async () => {
      await setMediaToQueue("Album", uuid);
      this.closeMenu();
    });
    this.addField("Add to playlist", async () => {
      this.removeFields();
      await this.spawnForSellectPlaylist(uuid, "Album");
    });
  }

  async spawnForPerformerInLibrary(uuid: string) {
    this.addField("Add to queue", async () => {
      await addMediaToQueue("Performer", uuid);
      this.closeMenu();
    });
    this.addField("Set queue", async () => {
      await setMediaToQueue("Performer", uuid);
      this.closeMenu();
    });
    this.addField("Add to playlist", async () => {
      this.removeFields();
      await this.spawnForSellectPlaylist(uuid, "Performer");
    });
  }

  async spawnForPlaylist(uuid: string) {
    this.addField("Add to queue", async () => {
      await addMediaToQueue("Playlist", uuid);
      this.closeMenu();
    });
    this.addField("Set queue", async () => {
      await setMediaToQueue("Playlist", uuid);
      this.closeMenu();
    });
    this.addField("Add to playlist", async () => {
      this.removeFields();
      await this.spawnForSellectPlaylist(uuid, "Playlist");
    });
  }

  async spawnForTrackInQueue(uuid: string) {
    this.addField("Start", async () => {
      await startTrack(uuid);
      this.closeMenu();
    });
    this.addField("Remove", async () => {
      await removeTrack(uuid);
      this.closeMenu();
    });
  }

  async spawnForSellectPlaylist(uuid: String, mediaType: MediaType) {
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
        await invoke("add_media_to_playlist", {
          playlistUuid: id,
          uuid: uuid,
          mediaType: mediaType
        });
        this.closeMenu();
      });
    })
    this.animate();
  }
}
