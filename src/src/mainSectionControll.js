const sections = Array.from(
  document.getElementsByClassName("section main-tab-section"),
);

const tabBinding = {
  "tracks-tab-button": "tracks-list-section",
  "albums-tab-button": "albums-list-section",
  "performers-tab-button": "performer-list-section",
  "queue-tab-button": "queue-section",
};

export function initMainSectionControll() {
  disable_all();

  Array.from(document.getElementsByClassName("left-menu-button")).forEach(
    (button) => {
      button.addEventListener("click", () => {
        disable_all();
        sections.find((x) => x.id == tabBinding[button.id]).style.display =
          "flex";
      });
    },
  );
}

function disable_all() {
  sections.forEach((section) => {
    section.style.display = "none";
  });
}
