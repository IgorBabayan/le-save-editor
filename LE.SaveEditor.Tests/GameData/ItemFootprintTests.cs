using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Tests.GameData;

public class ItemFootprintTests
{
    [Theory]
    [InlineData(0, 2, 2)]   // helmet
    [InlineData(1, 2, 3)]   // body armour
    [InlineData(16, 2, 4)]  // two-handed sword
    [InlineData(21, 1, 1)]  // ring
    [InlineData(31, 4, 1)]  // ornate idol
    [InlineData(41, 2, 2)]  // idol altar
    public void Of_KnownType_GivesItsSize(int baseTypeId, int width, int height) =>
        Assert.Equal(new ItemFootprint(width, height), ItemFootprint.Of(baseTypeId));

    [Theory]
    [InlineData(-1)]
    [InlineData(40)]
    [InlineData(255)]
    public void Of_UnknownType_IsSingleCell(int baseTypeId) =>
        Assert.Equal(ItemFootprint.Single, ItemFootprint.Of(baseTypeId));
}
