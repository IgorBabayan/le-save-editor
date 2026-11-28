using System.Text.Json;
using LE.SaveEditor.Services.Saves;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Tests.Saves;

public class CharacterSaveParserTests
{
    internal const string SampleJson =
        """{"characterName":"Hero","level":72,"slot":2,"hardcore":true,"soloChallenge":false,"soloCharacterChallenge":true,"cycle":8,"id":"2","characterClass":3,"chosenMastery":1,"lastPlayed":"2026-11-02T10:52:37.1234567891Z","savedItems":[{"itemData":null,"data":[2,0,3,4,0,10,20,30,50,0,0],"inventoryPosition":{"x":1,"y":2},"quantity":1,"containerID":1,"formatVersion":2},{"data":[300]},"bad",{"data":[6,1,2,3,4,21],"containerID":5,"formatVersion":6,"quantity":3}],"savedQuests":[{"questID":10,"questStepID":2},{"questID":"x"}],"unlockedWaypointScenes":["A0Z1","",5,"B1"]}""";

    private readonly CharacterSaveParser _parser = new();

    [Theory]
    [InlineData("EPOCH{\"a\":1}")]
    [InlineData("﻿  EPOCH  {\"a\":1}")]
    [InlineData("{\"a\":1}")]
    public void StripEpochPrefix_RemovesMarker(string content) =>
        Assert.Equal("{\"a\":1}", CharacterSaveParser.StripEpochPrefix(content));

    [Fact]
    public void Parse_ReadsCharacterFields()
    {
        var save = _parser.Parse("slot", "EPOCH" + SampleJson);

        Assert.Equal("slot", save.FilePath);
        Assert.Equal("Hero", save.CharacterName);
        Assert.Equal(72, save.Level);
        Assert.Equal(2, save.Slot);
        Assert.True(save.Hardcore);
        Assert.False(save.SoloSelfFound);
        Assert.True(save.SoloCharacterChallenge);
        Assert.Equal(8, save.Cycle);
        Assert.Equal("2", save.Id);
        Assert.Equal(3, (int)save.CharacterClass);
        Assert.Equal(1, save.ChosenMastery);
        Assert.NotNull(save.LastPlayed);
        Assert.Equal(2026, save.LastPlayed.Value.Year);
    }

    [Fact]
    public void Parse_SkipsInvalidSavedItemsButKeepsFileIndex()
    {
        var save = _parser.Parse("slot", SampleJson);

        Assert.Equal(2, save.SavedItems.Count);

        var first = save.SavedItems[0];
        Assert.Equal(0, first.Index);
        Assert.Equal(SavedItem.BackpackContainerId, first.ContainerId);
        Assert.Equal((1, 2), (first.X, first.Y));

        var second = save.SavedItems[1];
        Assert.Equal(3, second.Index);
        Assert.Equal(5, second.ContainerId);
        Assert.Equal(3, second.Quantity);
        Assert.Equal(6, second.FormatVersion);
    }

    [Fact]
    public void Parse_ReadsQuestsAndWaypointsLeniently()
    {
        var save = _parser.Parse("slot", SampleJson);

        Assert.Equal(new[] { new SavedQuest(10, 2) }, save.SavedQuests);
        Assert.Equal(new[] { "A0Z1", "B1" }, save.UnlockedWaypointScenes);
    }

    [Fact]
    public void Parse_NumericId_IsKeptAsText() =>
        Assert.Equal("7", _parser.Parse("slot", """{"id":7}""").Id);

    [Fact]
    public void Parse_MissingCollections_AreEmpty()
    {
        var save = _parser.Parse("slot", "{}");

        Assert.Equal(string.Empty, save.CharacterName);
        Assert.Empty(save.SavedItems);
        Assert.Empty(save.SavedQuests);
        Assert.Empty(save.UnlockedWaypointScenes);
    }

    [Fact]
    public void Parse_InvalidJson_Throws() =>
        Assert.ThrowsAny<JsonException>(() => _parser.Parse("slot", "EPOCH not json"));

    [Fact]
    public void Parse_SetsContentHashOfOriginalText()
    {
        var content = "EPOCH" + SampleJson;
        Assert.Equal(CharacterSaveParser.ComputeContentHash(content), _parser.Parse("slot", content).ContentHash);
        Assert.NotEqual(CharacterSaveParser.ComputeContentHash(content), CharacterSaveParser.ComputeContentHash(SampleJson));
    }

    [Fact]
    public async Task ReadAsync_ReadsFile()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, "EPOCH" + SampleJson);
            var save = await _parser.ReadAsync(path);
            Assert.Equal("Hero", save.CharacterName);
            Assert.Equal(path, save.FilePath);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
