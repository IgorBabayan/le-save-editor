using System.IO;
using Avalonia.Platform.Storage;

namespace LE.SaveEditor.Presentation.Services;

/// <summary>Native folder picker of the platform (Windows, GTK / portal on Linux, NSOpenPanel on macOS).</summary>
public sealed class FolderPickerService : IFolderPickerService
{
    public string? PickFolder(string title, string? initialDirectory = null)
    {
        var owner = WindowHelper.GetActiveWindow();
        if (owner is null)
            return null;

        var storage = owner.StorageProvider;
        if (!storage.CanPickFolder)
            return null;

        IStorageFolder? start = null;
        if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
            start = WindowHelper.Wait(storage.TryGetFolderFromPathAsync(initialDirectory));

        var folders = WindowHelper.Wait(storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            SuggestedStartLocation = start
        }));

        return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
    }
}
