using System;
using System.IO;
using System.IO.Pipes;

namespace Avalonix.Services.CommandLineInitializer;

public class PipeClient
{
    public PipeClient()
    {
        if (Environment.GetCommandLineArgs().Length < 2 || !File.Exists(Environment.GetCommandLineArgs()[1])) return;
        using var pipeClient = new NamedPipeClientStream(".", "AvalonixPipe", PipeDirection.InOut);
        pipeClient.Connect();
        using var writer = new StreamWriter(pipeClient);
        writer.WriteLine(Environment.GetCommandLineArgs()[1]);
    }
}