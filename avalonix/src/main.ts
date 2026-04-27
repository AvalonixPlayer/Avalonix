import { invoke } from "@tauri-apps/api/core";

invoke("get_tracks_ids")
  .then((ids) => console.log("tracks ids: " + ids))
  .catch((e) => console.error(e));

invoke("get_albums_ids")
  .then((ids) => console.log("albums ids: " + ids))
  .catch((e) => console.error(e));
