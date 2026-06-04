#[cfg(test)]
mod tests {
    use std::env;

    use anyhow::Result;

    use crate::{
        disk::db::DB,
        logger::{debug, error},
        media::{
            album::Album,
            media_trait::MediaType::{self},
            performer::Performer,
            track::Track,
        },
    };

    #[test]
    fn test_db() -> Result<()> {
        let args: Vec<String> = env::args().collect();
        let db = DB::open()?;
        let every_tracks_in_db: &[Track] = &db.get_every_track()?[0..];

        if let Ok(media_path) = std::env::var("ADD_MEDIA") {
            let tracks = Track::get_tracks_by_path(&media_path, every_tracks_in_db, &db)?;
            let albums = Album::create_albums(&db, &db.get_every_track()?[0..])?;
            let performers = Performer::create_performers(&db, &db.get_every_track()?[0..])?;
        }

        db.update();
        let tracks = db.get_every_track()?;
        let albums = db.get_every_album()?;
        let performers = db.get_every_performer()?;
        println!("tracks: {}", tracks.len());
        println!("albums: {}", albums.len());
        println!("performers: {}", performers.len());

        for i in tracks {
            debug(i);
        }

        for i in albums {
            debug(i);
        }

        for i in performers {
            debug(i);
        }
        Ok(())
    }
}
