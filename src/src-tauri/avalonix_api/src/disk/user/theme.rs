use serde::{Deserialize, Serialize};
use ts_rs::TS;

#[derive(Debug, Clone, Deserialize, Serialize, TS)]
#[ts(export)]
pub struct Theme {
    pub path_to_background_image: Option<String>,
    pub background_color: String,
    pub use_background_image: bool,
    pub bg_blur: i8,
    pub button_hover_color: String,
    pub button_active_color: String,
    pub sliders_color: String,
    pub font_size1: f32,
    pub font_size2: f32,
    pub font_size3: f32,
    pub font_size4: f32,
}

impl Theme {
    pub fn new() -> Self {
        Self {
            path_to_background_image: None,
            background_color: String::new(),
            use_background_image: false,
            button_hover_color: "#65547D".to_string(),
            button_active_color: "#7F68A2CC".to_string(),
            sliders_color: "#460077".to_string(),
            bg_blur: 0,
            font_size1: 40.0,
            font_size2: 32.0,
            font_size3: 25.0,
            font_size4: 20.0,
        }
    }

    pub fn from(
        path_to_background_image: Option<String>,
        background_color: String,
        use_background_image: bool,
        button_hover_color: String,
        button_active_color: String,
        sliders_color: String,
        bg_blur: i8,
        font_size1: f32,
        font_size2: f32,
        font_size3: f32,
        font_size4: f32,
    ) -> Self {
        Self {
            path_to_background_image,
            background_color,
            use_background_image,
            button_hover_color,
            button_active_color,
            sliders_color,
            bg_blur,
            font_size1,
            font_size2,
            font_size3,
            font_size4,
        }
    }
}
