using System;
using System.Collections.Generic;
using System.IO;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.Media.PlayBox;
using Avalonix.Services.CacheManager;
using Avalonix.Services.PlayableManager;
using Avalonix.Services.PlayableManager.PlayboxManager;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.CommandLineInitializer;

public class CommandLineInitializer(
    IPlayablesManager playablesManager,
    IMediaPlayer mediaPlayer,
    ILogger logger,
    ISettingsManager settingsManager,
    ICacheManager cacheManager,
    IPlayboxManager playboxManager) : ICommandLineInitializer
{
    public void Initialize()
    {
        var args = Environment.GetCommandLineArgs();
        foreach (var VARIABLE in args)
        {
            Console.WriteLine(VARIABLE);
        }

        if (args.Length > 1 && File.Exists(args[1]))
        {
            var paths = new List<string> { args[1] };
            playablesManager.StartPlayable(new Playbox(paths, mediaPlayer, logger,
                settingsManager.Settings!.Avalonix.PlaySettings, cacheManager));
        }
    }
}