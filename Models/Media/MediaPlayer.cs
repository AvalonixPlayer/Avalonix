using System;
using System.ComponentModel.DataAnnotations;
using Un4seen.Bass;

namespace Avalonix.API;

public class MediaPlayer
{
    private int _stream;

    public MediaPlayer() =>
        Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

    public void Play(Track track)
    {
        _stream = Bass.BASS_StreamCreateFile(track.TrackData.Path, 0, 0, BASSFlag.BASS_DEFAULT);

        if (_stream == 0)
        {
            Console.WriteLine($"Error: Could not create stream {track.TrackData.Path}");
            return;
        }

        Bass.BASS_ChannelPlay(_stream, false);

        Console.WriteLine($"Now playing {track.Metadata.TrackName}");

        ChangeVolume(50);
    }

    public void Stop() => 
        Bass.BASS_ChannelFree(_stream);
    
    public void Pause() =>
        Bass.BASS_ChannelPause(_stream);
    
    public void Resume() =>
        Bass.BASS_ChannelPlay(_stream, true);
    
    public void ChangeVolume(int volume) => // the volume should be in the range from 0 to 100
        Bass.BASS_ChannelSetAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, volume / 100F);
}