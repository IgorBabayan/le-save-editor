using LE.SaveEditor.Services.Theming;

namespace LE.SaveEditor.Services.Settings;

/// <summary>User preferences persisted between application runs.</summary>
public sealed record AppSettings
{
    public AppTheme Theme { get; init; } = AppTheme.DefaultNight;

    /// <summary>Folder with Last Epoch offline character saves.</summary>
    public string? SaveFolderPath { get; init; }
}
