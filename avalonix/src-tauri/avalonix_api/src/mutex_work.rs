use std::sync::{Arc, Mutex};

pub trait CreateArcMutex<T> {
    fn create_arc_mutex(self) -> Arc<Mutex<Self>>;
}

impl<T> CreateArcMutex<T> for T {
    fn create_arc_mutex(self) -> Arc<Mutex<Self>> {
        let arc = Arc::new(Mutex::new(self));
        arc
    }
}
