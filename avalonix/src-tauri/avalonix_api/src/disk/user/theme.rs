use serde::{Deserialize, Serialize};

#[derive(Debug, Clone, Deserialize, Serialize)]
pub struct Theme {
    pub path_to_background_image: Option<String>,
    pub background_color: (u16, u16, u16),
    pub use_background_image: bool,
    pub font_size: f32,
}

impl Theme {
    pub fn new() -> Self {
        Self {
            path_to_background_image: None,
            background_color: (255, 255, 255),
            use_background_image: false,
            font_size: 1.0,
        }
    }

    pub fn from(
        path_to_background_image: Option<String>,
        background_color: (u16, u16, u16),
        use_background_image: bool,
        font_size: f32,
    ) -> Self {
        Self {
            path_to_background_image,
            background_color,
            use_background_image,
            font_size,
        }
    }
}
