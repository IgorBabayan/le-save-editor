namespace LE.SaveEditor.DB.Infrastructure;

/// <summary>
/// Where the generator writes its output: the editor's own config folder
/// <c>%APPDATA%\LE.SaveEditor</c> (next to <c>settings.json</c> and <c>notifications.json</c>).
/// </summary>
internal sealed record ConfigPaths(string ConfigDirectory)
{
    public const string DatabaseFileName = "base.db";
    public const string IconsFolderName = "Icons";

    public string DatabaseFile => Path.Combine(ConfigDirectory, DatabaseFileName);

    public string IconsDirectory => Path.Combine(ConfigDirectory, IconsFolderName);

    /// <summary>Same folder <c>LE.SaveEditor</c> stores its settings in.</summary>
    public static string DefaultConfigDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LE.SaveEditor");

    /// <summary><c>--config &lt;folder&gt;</c> overrides the default folder (e.g. for testing).</summary>
    public static ConfigPaths Resolve(string[] args)
    {
        var index = Array.FindIndex(args, a => a is "--config" or "-c");
        return index >= 0 && index + 1 < args.Length
            ? new ConfigPaths(Path.GetFullPath(args[index + 1]))
            : new ConfigPaths(DefaultConfigDirectory);
    }
}
