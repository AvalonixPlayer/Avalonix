using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.Media.PlayBox;
using Avalonix.Services.CacheManager;
using Avalonix.Services.PlayableManager;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.CommandLineInitializer;

public class CommandLineInitializer(
    IPlayablesManager playablesManager,
    IMediaPlayer mediaPlayer,
    ILogger logger,
    ISettingsManager settingsManager,
    ICacheManager cacheManager) : ICommandLineInitializer
{

    private readonly string _appGuid = "AvalonixCLI";
    public void Initialize()
    {

        if (IsNew())
            logger.LogInformation($"Firstly initializing");
        else
        {
            logger.LogInformation($"Secondary initializing");
            Console.ReadLine();
            Environment.Exit(1);
        }


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

    private static Mutex? _instanceCheckMutex;
    private bool IsNew()
    {
        bool result;
        var mutex = new Mutex(true, _appGuid, out result);
        if (result)
            _instanceCheckMutex = mutex;
        else
            mutex.Dispose();
        return result;
    }
    
    
}