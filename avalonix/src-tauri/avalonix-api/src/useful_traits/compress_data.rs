pub trait CompressData {
    fn compress_data(&mut self);
}

impl CompressData for Vec<u8> {
    fn compress_data(&mut self) {}
}
