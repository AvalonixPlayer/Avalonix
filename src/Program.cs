using Avalonia;
using System;
using NeoSimpleLogger;
using AvalonixAPI;
using System.Threading;

namespace Avalonix;


public static class Program
{
        public static Logger Logger = new(Logger.TypeLogger.Console);

        [STAThread]
        public static void Main(string[] args)
        {
                // тестовый поток
                Thread th = new Thread(() =>
                {
                        MediaPlayer.Play(@"F:\Плейлисты\Простой\Geoxor - Ephermal.mp3");
                        Thread.Sleep(4000);
                        MediaPlayer.Pause();
                        Thread.Sleep(1000);
                        MediaPlayer.Continue();
                        Thread.Sleep(1000);
                        MediaPlayer.Stop();
                });
                th.Start();

                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        private static AppBuilder BuildAvaloniaApp()
        {

                Logger.Info("Building App");
#if DEBUG
                Logger.CallStack = true;
#endif
                return AppBuilder.Configure<App>()
                    .UsePlatformDetect()
                    .WithInterFont()
                    .LogToTrace();

        }
}
