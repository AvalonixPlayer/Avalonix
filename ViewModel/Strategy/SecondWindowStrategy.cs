using System.Threading.Tasks;
using Avalonix.Model.Media;

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


