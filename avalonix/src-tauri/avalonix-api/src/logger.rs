use colored::Colorize;

pub fn info(msg: &str) {
    println!("{} {}", "INFO".green(), msg);
}

pub fn error(msg: &str) {
    println!("{} {}", "ERROR".red(), msg);
}

pub fn acceptable_error(msg: &str) {
    println!("{} {}", "ACCEPTABLE ERROR".purple(), msg);
}

pub fn debug(msg: &str) {
    println!("{} {}", "DEBUG".blue(), msg);
}

pub fn fatal(msg: &str) {
    println!("{} {}", "FATAL".red().bold(), msg);
}

pub fn warn(msg: &str) {
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
