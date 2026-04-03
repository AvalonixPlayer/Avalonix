use lofty::picture::Picture;
use rustc_serialize::base64::{MIME, ToBase64};

pub trait CreateUri {
    fn create_uri(&self) -> String;
}

impl CreateUri for Vec<u8> {
    // ty https://gist.github.com/nathamanath/a5bda4bdbd07e579188f
    fn create_uri(&self) -> String {
        let b64 = self.to_base64(MIME);
        format!("data:image/{};base64,{}", "jpg", b64)
    }
}

impl CreateUri for Picture {
    fn create_uri(&self) -> String {
        let buf = self.data().to_owned();
        let b64 = buf.to_base64(MIME);
        format!("data:image/{};base64,{}", "jpg", b64)
    }
}
