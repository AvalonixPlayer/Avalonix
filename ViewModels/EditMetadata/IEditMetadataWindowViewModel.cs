using System.Threading.Tasks;
using Avalonia.Controls;

namespace Avalonix.ViewModels.EditMetadata;

public interface IEditMetadataWindowViewModel
{
    Task<string?> OpenTrackFileDialogAsync(Window parent);
}