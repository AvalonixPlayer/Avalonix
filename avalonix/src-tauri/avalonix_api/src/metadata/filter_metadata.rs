pub trait FilterMetadata {
    type Output;
    fn get_filter_metadata(&self) -> anyhow::Result<Self::Output>;
}
