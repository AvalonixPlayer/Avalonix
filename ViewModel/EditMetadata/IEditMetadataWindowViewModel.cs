using System.Threading.Tasks;
using Avalonia.Controls;

namespace Avalonix.ViewModel.EditMetadata;

public interface IEditMetadataWindowViewModel
{
    Task<string?> OpenTrackFileDialogAsync(Window parent);
}
