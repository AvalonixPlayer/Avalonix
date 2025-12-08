using System.IO;
using System.Threading.Tasks;
using Avalonix.Model;
using Avalonix.Services.DiskLoader;
using Avalonix.Services.DiskWriter;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.StatisticManager;

public class StatisticManager : IStatisticManager
{
    private readonly ILogger _logger;

    private readonly IDiskLoader _diskLoader;
    private readonly IDiskWriter _diskWriter;

    private readonly string _pathToStatisticsFile =
        Path.Combine(DiskManager.DiskManager.AvalonixFolderPath, "statistic" + DiskManager.DiskManager.Extension);

    public Statistic Statistic { get; private set; }

    public StatisticManager(ILogger logger, IDiskLoader diskLoader, IDiskWriter diskWriter)
    {
        if (!Directory.Exists(DiskManager.DiskManager.AvalonixFolderPath))
            Directory.CreateDirectory(DiskManager.DiskManager.AvalonixFolderPath);
        if (!Path.Exists(_pathToStatisticsFile))
            File.Create(_pathToStatisticsFile).Close();

        _diskLoader = diskLoader;
        _diskWriter = diskWriter;
        _logger = logger;

        Statistic = Task.Run(async () => await LoadStatistics()).Result;
    }

    private async Task<Statistic> LoadStatistics()
    {
        Statistic = await _diskLoader.LoadAsyncFromJson<Statistic>(_pathToStatisticsFile) ?? null!;
        return Statistic;
    }

    public async Task SaveStatistics()
    {
        _logger.LogInformation("Saving statistics");
        await _diskWriter.WriteJsonAsync(Statistic, _pathToStatisticsFile);
        _logger.LogInformation("Statistics saved");
    }
}