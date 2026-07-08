const sections = Array.from(
  document.getElementsByClassName("section main-tab-section"),
);

const tabBinding = {
  "tracks-tab-button": "tracks-list-section",
  "albums-tab-button": "albums-list-section",
  "performers-tab-button": "performers-list-section",
  "queue-tab-button": "queue-section",
  "current-track-show-button": "track-preview-section",
};

export function initMainSectionControll() {
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
