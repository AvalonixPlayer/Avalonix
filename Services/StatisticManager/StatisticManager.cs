using System.IO;
using System.Threading.Tasks;
using Avalonix.Model;
using Avalonix.Services.DiskLoader;
using Avalonix.Services.DiskWriter;

namespace Avalonix.Services.StatisticManager;

public class StatisticManager : IStatisticManager
{
    private readonly IDiskLoader _diskLoader;
    private readonly IDiskWriter _diskWriter;

    private readonly string _pathToStatisticsFile =
        Path.Combine(DiskManager.DiskManager.AvalonixFolderPath, "statistic" + DiskManager.DiskManager.Extension);
    
    public Statistic Statistic {get; private set;}

    public StatisticManager(IDiskLoader diskLoader, IDiskWriter diskWriter)
    {
        if (!Directory.Exists(DiskManager.DiskManager.AvalonixFolderPath))
            Directory.CreateDirectory(DiskManager.DiskManager.AvalonixFolderPath);
        if (!Path.Exists(_pathToStatisticsFile))
            File.Create(_pathToStatisticsFile).Close();

        _diskLoader = diskLoader;
        _diskWriter = diskWriter;

        Statistic = LoadStatistics().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private async Task<Statistic> LoadStatistics()
    {
        Statistic = await _diskLoader.LoadAsyncFromJson<Statistic>(_pathToStatisticsFile) ?? null!;
        return Statistic;
    }

    public async Task SaveStatistics()
    {
        await _diskWriter.WriteJsonAsync(Statistic, _pathToStatisticsFile);
    }
}