using System.Text.Json;
using LE.SaveEditor.Services.Saves;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Tests.Saves;

public class CharacterSaveWriterTests
{
    private const string Content = "EPOCH" + CharacterSaveParserTests.SampleJson + "\n";

    private readonly CharacterSaveParser _parser = new();

    private CharacterSave Parse(string content = Content) => _parser.Parse("slot", content);

    [Fact]
    public void Apply_NoChanges_KeepsContent() =>
        Assert.Equal(Content, CharacterSaveWriter.Apply(Content, []));

    [Fact]
    public void Apply_KeepsPrefixAndSuffix()
    {
        var result = CharacterSaveWriter.Apply(Content, [], ["X"]);

        Assert.StartsWith("EPOCH{", result);
        Assert.EndsWith("}\n", result);
    }

    [Fact]
    public void Apply_ReplaceMovesItemAndKeepsUnknownProperties()
    {
        var item = Parse().SavedItems[0];
        var moved = item with { X = 4, Y = 5, Quantity = 2 };

        var result = CharacterSaveWriter.Apply(Content, [SavedItemUpdate.Replace(item, moved)]);
        var saved = _parser.Parse("slot", result).SavedItems[0];

        Assert.Equal((4, 5, 2), (saved.X, saved.Y, saved.Quantity));
        Assert.Contains("\"itemData\":null", result);
    }

    [Fact]
    public void Apply_RemoveAndAdd()
    {
        var save = Parse();
        var added = new SavedItem(SavedItem.BackpackContainerId, 0, 0, 1, 2, [2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

        var result = CharacterSaveWriter.Apply(Content,
            [SavedItemUpdate.Remove(save.SavedItems[0]), SavedItemUpdate.Add(added)]);
        var items = _parser.Parse("slot", result).SavedItems;

        Assert.Equal(2, items.Count);
        Assert.Equal(5, items[0].ContainerId);
        Assert.Equal(added.Data, items[1].Data);
    }

    [Fact]
    public void Apply_ReplacesWaypointsWithoutDuplicates()
    {
        var result = CharacterSaveWriter.Apply(Content, [], ["Z1", "Z1", " ", "Z2"]);
        Assert.Equal(new[] { "Z1", "Z2" }, _parser.Parse("slot", result).UnlockedWaypointScenes);
    }

    [Fact]
    public void Apply_UnknownIndex_Throws()
    {
        var ghost = Parse().SavedItems[0] with { Index = 99 };
        Assert.Throws<ArgumentException>(() => CharacterSaveWriter.Apply(Content, [SavedItemUpdate.Remove(ghost)]));
    }

    [Fact]
    public void Apply_SameItemTwice_Throws()
    {
        var item = Parse().SavedItems[0];
        Assert.Throws<ArgumentException>(() =>
            CharacterSaveWriter.Apply(Content, [SavedItemUpdate.Remove(item), SavedItemUpdate.Remove(item)]));
    }

    [Fact]
    public void Apply_NoJsonObject_Throws() =>
        Assert.Throws<JsonException>(() => CharacterSaveWriter.Apply("EPOCH", []));

    [Fact]
    public async Task SaveAsync_WritesFileAndBackup()
    {
        var folder = Directory.CreateTempSubdirectory("le-tests-").FullName;
        try
        {
            var path = Path.Combine(folder, "1CHARACTERSLOT_BETA_0");
            await File.WriteAllTextAsync(path, Content);
            var save = await _parser.ReadAsync(path);
            var writer = new CharacterSaveWriter(_parser);

            var result = await writer.SaveAsync(save, [SavedItemUpdate.Remove(save.SavedItems[0])]);

            Assert.True(File.Exists(result.BackupPath));
            Assert.Equal(Content, await File.ReadAllTextAsync(result.BackupPath));
            Assert.Single((await _parser.ReadAsync(path)).SavedItems);
            Assert.False(File.Exists(path + ".editor_temp"));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_FileChangedAfterLoad_Throws()
    {
        var folder = Directory.CreateTempSubdirectory("le-tests-").FullName;
        try
        {
            var path = Path.Combine(folder, "1CHARACTERSLOT_BETA_0");
            await File.WriteAllTextAsync(path, Content);
            var save = await _parser.ReadAsync(path);
            await File.WriteAllTextAsync(path, Content + " ");

            await Assert.ThrowsAsync<SaveFileChangedException>(() =>
                new CharacterSaveWriter(_parser).SaveAsync(save, []));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
