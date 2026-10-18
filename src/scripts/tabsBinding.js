import { fillAlbumsLibrary } from "./albumsCreator";
import { fillPerformersLibrary } from "./performerCreator";
import { fillPlaylistsLibrary as fillPlaylistsLibrary } from "./playlistsCreator";
import { fillTracksLibrary } from "./tracksCreator";

const sections = Array.from(
  document.getElementsByClassName("tab"),
);

const tabBinding = {
  "search-media-tab-button" : "media-search-tab",
  "tracks-tab-button": "tracks-list-tab",
  "albums-tab-button": "albums-list-tab",
  "performers-tab-button": "performers-list-tab",
  "playlists-tab-button": "playlists-list-tab",
  "queue-tab-button": "queue-list-tab",
  "settings-tab-button": "settings-tab",
  //"current-track-show-button": "track-preview-tab",
};

export async function initTabs() {
  disable_all();

  Object.entries(tabBinding).forEach((tab, i) => {
    document.getElementById(tab[0]).addEventListener("click", async () => {
      disable_all();
      sections.find((x) => x.id == tab[1]).style.display = "flex";
      if (i == 1) await fillTracksLibrary();
      if (i == 2) await fillAlbumsLibrary();
      if (i == 3) await fillPerformersLibrary();
      if (i == 4) await fillPlaylistsLibrary();
    });
  });
}

function disable_all() {
  sections.forEach((section) => {
    section.style.display = "none";
  });
}
