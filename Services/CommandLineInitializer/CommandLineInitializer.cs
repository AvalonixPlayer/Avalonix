using System;
using System.IO;
using System.Threading;
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

    // Don`t remove
    // ReSharper disable NotAccessedField.Local
    private PipeServer? _pipeServer;
    private PipeClient? _pipeClient;
    private static Mutex? _instanceCheckMutex;

    public void Initialize()
    {
        if (IsNew())
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && File.Exists(args[1]))
                StartPlayable(args[1]);
            _pipeServer = new PipeServer();
            _pipeServer.InformationReceived += s => { StartPlayable(s); };
        }
        else
        {
            _pipeClient = new PipeClient();
            Environment.Exit(1);
        }
    }

    private void StartPlayable(string path)
    {
        playablesManager.StartPlayable(new Playbox([path], mediaPlayer, logger,
            settingsManager.Settings!.Avalonix.PlaySettings, cacheManager));
    }


    private bool IsNew()
    {
        var mutex = new Mutex(true, _appGuid, out var result);
        if (result)
            _instanceCheckMutex = mutex;
        else
            mutex.Dispose();
        return result;
    }
}