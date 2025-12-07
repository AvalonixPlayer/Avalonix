using System.Threading.Tasks;
using Avalonix.Model;

namespace Avalonix.Services.StatisticManager;

public interface IStatisticManager
{
    Statistic Statistic { get; }
    Task SaveStatistics();
}