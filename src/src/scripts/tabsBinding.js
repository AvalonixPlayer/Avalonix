const sections = Array.from(
  document.getElementsByClassName("tab"),
);

const tabBinding = {
  "search-media-tab-button" : "media-search-tab",
  "tracks-tab-button": "tracks-list-tab",
  "albums-tab-button": "albums-list-tab",
  "performers-tab-button": "performers-list-tab",
  "playlists-tab-button": "playlists-tab",
  "queue-tab-button": "queue-tab",
  "settings-tab-button": "settings-tab",
  //"current-track-show-button": "track-preview-tab",
};

export function initTabs() {
  disable_all();

  Object.entries(tabBinding).forEach((tab, i) => {
    document.getElementById(tab[0]).addEventListener("click", () => {
      disable_all();
      sections.find((x) => x.id == tab[1]).style.display = "flex";
    });
  });
}

function disable_all() {
  sections.forEach((section) => {
    section.style.display = "none";
  });
}
