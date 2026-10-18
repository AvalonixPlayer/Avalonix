// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use avalonix_api::logger::fatal;
use avalonix_lib;

fn main() {
    avalonix_lib::run().map_err(|err| fatal(err)).unwrap();
}
