import { open } from '@tauri-apps/plugin-dialog';
export async function pickDir() {
  const file = await open({
    multiple: false,
    directory: true,
  });
  return file;
}
