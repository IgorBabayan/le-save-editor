namespace LE.SaveEditor.DB.Infrastructure;

/// <summary>
/// Where the generator writes its output: the <c>Config</c> folder of the <c>LE.SaveEditor</c> project
/// (copied next to the editor's exe on build/publish).
/// </summary>
internal sealed record ConfigPaths(string ConfigDirectory)
{
    public const string DatabaseFileName = "base.db";
    public const string IconsFolderName = "Icons";

    public string DatabaseFile => Path.Combine(ConfigDirectory, DatabaseFileName);

    public string IconsDirectory => Path.Combine(ConfigDirectory, IconsFolderName);

    /// <summary>
    /// <c>--config &lt;folder&gt;</c> wins; otherwise walks up from the exe until it finds
    /// <c>LE.SaveEditor/LE.SaveEditor.csproj</c> (works from <c>bin/Debug/net10.0</c> and from the IDE).
    /// </summary>
    public static ConfigPaths Resolve(string[] args)
    {
        var index = Array.FindIndex(args, a => a is "--config" or "-c");
        if (index >= 0 && index + 1 < args.Length)
            return new ConfigPaths(Path.GetFullPath(args[index + 1]));

        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var editorProject = Path.Combine(dir.FullName, "LE.SaveEditor", "LE.SaveEditor.csproj");
            if (File.Exists(editorProject))
                return new ConfigPaths(Path.Combine(dir.FullName, "LE.SaveEditor", "Config"));
        }

        throw new DirectoryNotFoundException(
            "Could not find the LE.SaveEditor project folder. Start the generator with --config <folder>.");
    }
}
