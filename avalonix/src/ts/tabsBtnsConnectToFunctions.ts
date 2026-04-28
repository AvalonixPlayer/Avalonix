let displays: String[] = [];

export function tabsBtnsConnectToFunctions() {
  let btns = document.querySelectorAll(".tab-sellect-button");

  btns.forEach((btn_el, index) => {
    let btn = btn_el as HTMLElement;
    btn.addEventListener("click", () => activateTab(index));
  });

  let sections = document.querySelectorAll("section.main-section-child");

  sections.forEach((section) => {
    displays.push((section as HTMLElement).style.display);
  });

  disable_all();
  activateTab(0);
}

function activateTab(index: number) {
  let sections = document.querySelectorAll("section.main-section-child");
  disable_all();

  (sections[index] as HTMLElement).style.display = displays[index].toString();
}

function disable_all() {
  let sections = document.querySelectorAll("section.main-section-child");

  sections.forEach((section) => {
    (section as HTMLElement).style.display = "none";
  });
}
