using System;
using System.IO;

namespace AvalonixAPI;

public static class DiskManager
{
    public static string EnvPath()
    {
        return Path.GetDirectoryName(Environment.ProcessPath) ?? "";
    }
}