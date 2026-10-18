export function regCustomElements() {
  customElements.define("drop-list", DropList)
}

export class DropList extends HTMLElement {
  constructor() {
    super();

    const template = document.getElementById("drop-list-template") as HTMLTemplateElement;
    let title = this.getAttribute("title");
    let addButton = this.getAttribute("add-button");
    const clone = template!.content.cloneNode(true) as HTMLElement;

    if (title == "" || !title) {
      title = "No title";
    }
    if (addButton == "" || !addButton) {
      clone.querySelector(".drop-list-add-button")!.remove();
    }
    else {
      clone.querySelector(".drop-list-add-button")!.id = addButton;
    }

    clone.querySelector("button")!.querySelector("h2")!.textContent = title;

    clone.querySelector("button")!.addEventListener("click", () => {
      const listContainer = this.querySelector(".drop-list-items") as HTMLElement;

      if (!listContainer) return;


      if (listContainer.style.display === "none") {
        listContainer.style.display = "flex";
      } else {
        console.log(listContainer.style.display);
        listContainer.style.display = "none";
      }
    });
    this.prepend(clone);
  }

  addItem(text: string): HTMLElement {
      let list = this.querySelector(".drop-list-items")!;
      let template = document.getElementById("drop-list-button-template") as HTMLTemplateElement;

      const fragment = template.content.cloneNode(true) as DocumentFragment;

      const element = fragment.firstElementChild as HTMLElement;

      element.querySelector("h3")!.textContent = text;

      list.appendChild(fragment);
      return element;
  }
}
