use logger::info;
use rust_discord_rpc::{DiscordRPC, EventHandlers, RichPresenceBuilder};
use std::time::SystemTime;

pub struct DiscordRpc {
    rpc: DiscordRPC,
}

impl DiscordRpc {
    pub fn new(client_id: &str) -> Self {
        let handlers = EventHandlers {
            ready: |user| {
                info!(
                    "Discord RPC initialized! User: {}#{}",
                    user.username, user.discriminator
                );
            },
            disconnected: |code, msg| {
                println!("Discord RPC disconnected: {} - {}", code, msg);
            },
            errored: |code, msg| {
                println!("Inner Discord RPC error: {} - {}", code, msg);
            },
            ..Default::default()
        };

        let rpc = DiscordRPC::init(client_id, true, None, handlers)
            .expect("Error while initializing Discord RPC");

        Self { rpc }
    }

    pub fn update_presence(&mut self, details: &str, state: &str) {
        let start_time = SystemTime::now()
            .duration_since(SystemTime::UNIX_EPOCH)
            .unwrap()
            .as_secs() as i64;

        let presence = RichPresenceBuilder::new()
            .state(state)
            .details(details)
            .large_image_key("large_image_name")
            .large_image_text("Avalonix")
            .start_timestamp(start_time)
            .build();

        self.rpc
            .update_presence(presence)
            .expect("Error while updating presence");
    }

    pub fn run_callbacks(&self) {
        self.rpc.run_callbacks();
    }
}
