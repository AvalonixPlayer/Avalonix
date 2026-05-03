use anyhow::Ok;
use avalonix_api::logger;
use tauri::{
    menu::{Menu, MenuItem},
    tray::{MouseButton, MouseButtonState, TrayIconBuilder, TrayIconEvent},
    App, AppHandle, Manager,
};

pub fn init_tray(app: &mut App) -> anyhow::Result<()> {
    let quit_i = MenuItem::with_id(app, "quit", "Quit", true, None::<&str>)?;
    let show_i = MenuItem::with_id(app, "show", "Show", true, None::<&str>)?;
    let menu = Menu::with_items(app, &[&quit_i, &show_i])?;

    let tray = TrayIconBuilder::new()
        .menu(&menu)
        .show_menu_on_left_click(false)
        .icon(app.default_window_icon().unwrap().clone())
        .build(app)?;

    tray.on_menu_event(|app, event| match event.id.as_ref() {
        "quit" => {
            logger::debug("exit");
            app.exit(0);
        }
        "show" => {
            show(app);
        }
        _ => {
            logger::error(format!("menu item {:?} not handled", event.id));
        }
    });

    tray.on_tray_icon_event(|tray, event| match event {
        TrayIconEvent::Click {
            button: MouseButton::Left,
            button_state: MouseButtonState::Up,
            ..
        } => {
            let app = tray.app_handle();
            show(app);
        }
        _ => {}
    });
    Ok(())
}

fn show(app: &AppHandle) {
    if let Some(window) = app.get_webview_window("main") {
        let _ = window.unminimize();
        let _ = window.show();
        let _ = window.set_focus();
    }
    logger::debug("show");
}
