use std::env;

pub fn get_argument_val(arg_name: &str) -> Option<String> {
    env::var(arg_name).ok()
}
