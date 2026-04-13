use colored::Colorize;
use std::fmt::Display;

pub fn info<T: Display>(msg: T) {
    println!("{} {}", "INFO".green(), msg);
}

pub fn error<T: Display>(msg: T) {
    println!("{} {}", "ERROR".red(), msg);
}

pub fn debug<T: Display>(msg: T) {
    println!("{} {}", "DEBUG".blue(), msg);
}

pub fn fatal<T: Display>(msg: T) {
    println!("{} {}", "FATAL".red().bold(), msg);
}

pub fn warn<T: Display>(msg: T) {
    println!("{} {}", "WARN".yellow(), msg);
}

#[test]
fn test_logger() {
    info("Test");
    error("Test");
    debug("Test");
    warn("Test");
    fatal("Test");
}
