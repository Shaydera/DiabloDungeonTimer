using System.Windows.Forms;
using DiabloDungeonTimer.Core.Services.Interfaces;

namespace DiabloDungeonTimer.UI.Services;

public class WindowsFileService : IFileService
{
    public string BrowseDirectory(string initialDirectory)
    {
        var dialog = new FolderBrowserDialog
        {
            InitialDirectory = initialDirectory,
            ShowNewFolderButton = false
        };
        DialogResult dialogResult = dialog.ShowDialog();
        return dialogResult == DialogResult.OK ? dialog.SelectedPath : string.Empty;
    }
}