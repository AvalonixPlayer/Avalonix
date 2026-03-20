const { invoke } = window.__TAURI__.core;

let greetInputEl;
let greetMsgEl;

async function greet() {
  // Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
  greetMsgEl.textContent = await invoke("greet", { name: greetInputEl.value });
}

window.addEventListener("DOMContentLoaded", () => {

});

invoke('get_all_tracks').then((messge) => console.log(messge));

setInterval(() => {
  let x = window.screenX /2 + window.innerWidth / 2;
  let y = window.screenY /2  + window.innerHeight /2;
  const col1 = getComputedStyle(document.documentElement).getPropertyValue("--background-gradient1");
  const col2 = getComputedStyle(document.documentElement).getPropertyValue("--background-gradient2");
	document.body.style.background = `radial-gradient(circle 350px at ${x}px ${y}px, ${col1}, ${col2} )`;

}, 10);