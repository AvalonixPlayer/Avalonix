using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvalonixAPI;

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