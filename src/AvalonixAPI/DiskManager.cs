using System;
using System.IO;

namespace Avalonix.AvalonixAPI;

public static class DiskManager
{
    public static string SettingsPath
    {
        get
        {
            string result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".avalonix");
            if (!Path.Exists(result)) Directory.CreateDirectory(result);
            return result;
        }
    }
}