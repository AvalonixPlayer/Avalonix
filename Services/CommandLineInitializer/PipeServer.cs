using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Avalonix.Services.CommandLineInitializer;

public sealed class PipeServer
{
    public Action<string>? InformationReceived;

    public PipeServer()
    {
        Task.Run(ActivateServer);
    }

    private void ActivateServer()
    {
        while (true)
        {
            using var pipeServer = new NamedPipeServerStream("AvalonixPipe", PipeDirection.InOut);
            pipeServer.WaitForConnection();
            using var reader = new StreamReader(pipeServer);
            while (reader.ReadLine() is { } temp) InformationReceived!.Invoke(temp);
        }
        // ReSharper disable once FunctionNeverReturns
    }
}