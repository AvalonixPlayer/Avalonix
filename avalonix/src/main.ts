import {albumsElements, allTracks, appendTracksList, getAllAlbums, getAllArtists, getAllTracks} from './playboxesFiller';

await getAllTracks();
await getAllAlbums();
await getAllArtists();
await appendTracksList();

setInterval(() => {
  setBG();
}, 10);

async function setBG() {
  let x = window.screenX /2 + window.innerWidth / 2;
  let y = window.screenY /2  + window.innerHeight /2;
  const col1 = getComputedStyle(document.documentElement).getPropertyValue("--background-gradient1");
  const col2 = getComputedStyle(document.documentElement).getPropertyValue("--background-gradient2");
	document.body.style.background = `radial-gradient(circle 350px at ${x}px ${y}px, ${col1}, ${col2} )`;
}