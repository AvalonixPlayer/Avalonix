using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Avalonix.Services.VersionManager;

public class VersionManager : IVersionManager
{
    private const string ReleaseUrl = "https://github.com/AvalonixPlayer/Avalonix/blob/main/Release";

    public async Task<Release> GetLastRelease()
    {
        string version;
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(ReleaseUrl);
            var content = await response.Content.ReadAsStringAsync();
            version = CutText(content, "AvalonixTAG_Version", "AvalonixTAG_Close");
        }

        return new Release(version);
    }

    public async Task<Release> GetCurrentRelease()
    {
        var pathToRelease = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!,"Release");
        if(!Path.Exists(pathToRelease)) return new Release("null");
        var content = await File.ReadAllTextAsync(pathToRelease);
        var version = CutText(content, "AvalonixTAG_Version", "AvalonixTAG_Close");
        return new Release(version);
    }

    private static string CutText(string input, string start, string end)
    {
        var pattern = $"(?<={start}).*?(?={end})";
        var regex = new Regex(pattern);
        var match = regex.Match(input);
        return match.Groups[0].Value;
    }
}

public class Release(string version)
{
    public string Version => version;
}
