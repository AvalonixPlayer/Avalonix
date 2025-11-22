using System.Threading.Tasks;
using Avalonix.Services.Media;

namespace Avalonix.ViewModels.Strategy;

public interface ISecondWindowStrategy
{
    string WindowTitle { get; }
    string ActionButtonText { get; }
}

public interface IPlayableWindowStrategy : ISecondWindowStrategy
{
    Task ExecuteAsync(IPlayable playlist);
}


