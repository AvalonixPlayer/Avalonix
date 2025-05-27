namespace AvalonixAPI;

public static class DiskManager
{
    public static string EnvPath() => Path.GetDirectoryName(Environment.ProcessPath) ?? "";
}
