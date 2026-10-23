using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

/// <summary>
/// Writes <c>savedItems</c> changes into a character save. The file is read again and only the changed entries of
/// <c>savedItems</c> are touched: every other field, unknown properties of the entries and the entry order stay as the
/// game wrote them. The text is written with the game's layout (<c>EPOCH</c> marker, two-space indentation, the file's
/// line endings) to a temporary file that then replaces the save, keeping the old file as a timestamped backup.
/// </summary>
public sealed partial class CharacterSaveWriter(ICharacterSaveParser parser) : ICharacterSaveWriter
{
    /// <summary>Backups kept per save file; older ones are deleted after a successful save.</summary>
    public const int BackupsToKeep = 5;

    private const string BackupTimestampFormat = "yyyyMMdd-HHmmss";

    /// <summary>Contains <c>_temp</c>, so <see cref="SaveFolderScanner"/> never lists a leftover as a character.</summary>
    private const string TempSuffix = ".editor_temp";

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    [GeneratedRegex(@"\.\d{8}-\d{6}\.bak$", RegexOptions.IgnoreCase)]
    private static partial Regex BackupSuffix();

    public async Task<CharacterSaveResult> SaveAsync(
        CharacterSave character,
        IReadOnlyCollection<SavedItemUpdate> updates,
        CancellationToken cancellationToken = default)
    {
        var path = character.FilePath;
        var content = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);

        // Entries are matched by their index in the file, which only holds while the file is the one that was read.
        if (!string.Equals(CharacterSaveParser.ComputeContentHash(content), character.ContentHash, StringComparison.Ordinal))
            throw new SaveFileChangedException(path);

        var newContent = Apply(content, updates);

        // Never write something the editor itself cannot read back.
        var saved = parser.Parse(path, newContent);

        cancellationToken.ThrowIfCancellationRequested();
        var tempPath = path + TempSuffix;
        await File.WriteAllTextAsync(tempPath, newContent, Utf8NoBom, cancellationToken).ConfigureAwait(false);

        var backupPath = await Task.Run(() => ReplaceWithBackup(tempPath, path), CancellationToken.None).ConfigureAwait(false);
        return new CharacterSaveResult(saved, backupPath);
    }

    /// <summary>The file content with <paramref name="updates"/> applied to its <c>savedItems</c>.</summary>
    public static string Apply(string content, IReadOnlyCollection<SavedItemUpdate> updates)
    {
        // Keep whatever surrounds the JSON body ("EPOCH" in front, a final line break) exactly as it is.
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        if (start < 0 || end < start)
            throw new JsonException("The save file holds no JSON object.");

        var prefix = content[..start];
        var suffix = content[(end + 1)..];
        var newLine = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";

        if (JsonNode.Parse(content[start..(end + 1)], documentOptions: DocumentOptions) is not JsonObject root)
            throw new JsonException("The save file holds no JSON object.");

        ApplyToSavedItems(root, updates);

        var json = root.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true,
            NewLine = newLine,
            // Write names and texts as they are instead of \u-escaping quotes, '+', '<', accents, ...
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        return prefix + json + suffix;
    }

    private static void ApplyToSavedItems(JsonObject root, IReadOnlyCollection<SavedItemUpdate> updates)
    {
        if (updates.Count == 0)
            return;

        JsonArray savedItems;
        switch (root["savedItems"])
        {
            case JsonArray array:
                savedItems = array;
                break;
            case null:
                savedItems = [];
                root["savedItems"] = savedItems;
                break;
            default:
                throw new JsonException("savedItems is not an array.");
        }

        // Index in the file → its replacement (null = removed).
        var byIndex = new Dictionary<int, SavedItemUpdate>();
        var added = new List<SavedItem>();
        foreach (var update in updates)
        {
            if (update.Original is null)
            {
                if (update.Replacement is not null)
                    added.Add(update.Replacement);
                continue;
            }

            var index = update.Original.Index;
            if (index < 0 || index >= savedItems.Count || savedItems[index] is not JsonObject)
                throw new ArgumentException($"Saved item #{index} is not in the file.", nameof(updates));
            if (!byIndex.TryAdd(index, update))
                throw new ArgumentException($"Saved item #{index} is changed twice.", nameof(updates));
        }

        // Detach every entry so the kept ones can be put back in their order.
        var entries = savedItems.ToList();
        savedItems.Clear();

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (!byIndex.TryGetValue(i, out var update))
            {
                savedItems.Add(entry);
                continue;
            }

            if (update.Replacement is null)
                continue;

            var entryObject = (JsonObject)entry!;
            Update(entryObject, update.Original!, update.Replacement);
            savedItems.Add(entryObject);
        }

        foreach (var item in added)
            savedItems.Add(CreateEntry(item));
    }

    /// <summary>Changes only the fields that differ, so unknown properties and untouched values stay as written.</summary>
    private static void Update(JsonObject entry, SavedItem original, SavedItem replacement)
    {
        if (!original.Data.SequenceEqual(replacement.Data))
            entry["data"] = ToJsonArray(replacement.Data);

        if (original.X != replacement.X || original.Y != replacement.Y)
        {
            if (entry["inventoryPosition"] is JsonObject position)
            {
                position["x"] = replacement.X;
                position["y"] = replacement.Y;
            }
            else
            {
                entry["inventoryPosition"] = new JsonObject { ["x"] = replacement.X, ["y"] = replacement.Y };
            }
        }

        if (original.ContainerId != replacement.ContainerId)
            entry["containerID"] = replacement.ContainerId;
        if (original.Quantity != replacement.Quantity)
            entry["quantity"] = replacement.Quantity;
        if (original.FormatVersion != replacement.FormatVersion)
            entry["formatVersion"] = replacement.FormatVersion;
    }

    /// <summary>A new entry with the properties (and their order) the game writes.</summary>
    private static JsonObject CreateEntry(SavedItem item) => new()
    {
        ["itemData"] = null,
        ["data"] = ToJsonArray(item.Data),
        ["inventoryPosition"] = new JsonObject { ["x"] = item.X, ["y"] = item.Y },
        ["quantity"] = item.Quantity,
        ["containerID"] = item.ContainerId,
        ["formatVersion"] = item.FormatVersion
    };

    private static JsonArray ToJsonArray(IReadOnlyList<byte> data)
    {
        var array = new JsonArray();
        foreach (var b in data)
            array.Add((int)b);
        return array;
    }

    /// <summary>Puts <paramref name="tempPath"/> in place of <paramref name="path"/>, keeping the old file as a backup.</summary>
    /// <returns>Path of the backup.</returns>
    private static string ReplaceWithBackup(string tempPath, string path)
    {
        var backupPath = $"{path}.{DateTime.Now.ToString(BackupTimestampFormat, CultureInfo.InvariantCulture)}.bak";
        try
        {
            try
            {
                File.Replace(tempPath, path, backupPath, ignoreMetadataErrors: true);
            }
            catch (PlatformNotSupportedException)
            {
                // File systems without an atomic replace.
                File.Copy(path, backupPath, overwrite: true);
                File.Move(tempPath, path, overwrite: true);
            }
        }
        catch
        {
            TryDelete(tempPath);
            throw;
        }

        DeleteOldBackups(path);
        return backupPath;
    }

    /// <summary>Keeps the newest <see cref="BackupsToKeep"/> backups the editor made of <paramref name="path"/>.</summary>
    private static void DeleteOldBackups(string path)
    {
        try
        {
            var folder = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(folder))
                return;

            var fileName = Path.GetFileName(path);
            var old = Directory.EnumerateFiles(folder, fileName + ".*.bak")
                .Where(f => BackupSuffix().IsMatch(f)
                            && Path.GetFileName(f).Length == fileName.Length + ".yyyyMMdd-HHmmss.bak".Length)
                .OrderByDescending(f => f, StringComparer.OrdinalIgnoreCase)
                .Skip(BackupsToKeep);

            foreach (var file in old)
                TryDelete(file);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Old backups are only clutter; never fail a save for them.
        }
    }

    private static void TryDelete(string file)
    {
        try
        {
            File.Delete(file);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }
}
