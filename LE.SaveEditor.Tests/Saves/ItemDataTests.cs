using LE.SaveEditor.Services.Saves;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Tests.Saves;

public class ItemDataTests
{
    // v2 rare helmet: type 0, sub 3, rarity 4, flags 0, implicits 10/20/30, FP 50, 2 affixes (T5 id 0x123 roll 200, T1 id 7 roll 0), tail 0.
    private static readonly byte[] PlainV2 = [2, 0, 3, 4, 0, 10, 20, 30, 50, 2, 0x41, 0x23, 200, 0x00, 7, 0, 0];

    // v6 unique ring: instance id 1..4, type 21, sub 1, rarity 7, flags 0, implicits, uniqueID 0x0102, 8 rolls, LP 3.
    private static readonly byte[] UniqueV6 = [6, 1, 2, 3, 4, 21, 1, 7, 0, 5, 6, 7, 0x01, 0x02, 1, 2, 3, 4, 5, 6, 7, 8, 3];

    [Fact]
    public void Read_PlainV2_DecodesHeaderAndAffixes()
    {
        var item = ItemDataReader.Read(PlainV2);

        Assert.NotNull(item);
        Assert.Equal(2, item.FormatVersion);
        Assert.Equal(0, item.BaseTypeId);
        Assert.Equal(3, item.SubTypeId);
        Assert.Equal(4, item.RarityCode);
        Assert.Equal(new byte[] { 10, 20, 30 }, item.ImplicitRolls);
        Assert.Equal(50, item.ForgingPotential);
        Assert.Null(item.UniqueId);
        Assert.False(item.IsUnique);
        Assert.Equal(2, item.Affixes.Count);
        Assert.Equal(new ItemAffixData(0x123, 5, 200, false), item.Affixes[0]);
        Assert.Equal(new ItemAffixData(7, 1, 0, false), item.Affixes[1]);
        Assert.Equal((byte)0, item.Tail);
    }

    [Fact]
    public void Read_UniqueV6_DecodesInstanceIdAndUniqueId()
    {
        var item = ItemDataReader.Read(UniqueV6);

        Assert.NotNull(item);
        Assert.Equal(6, item.FormatVersion);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, item.InstanceId!);
        Assert.Equal(21, item.BaseTypeId);
        Assert.Equal(0x0102, item.UniqueId);
        Assert.True(item.IsUnique);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, item.UniqueRolls);
        Assert.Empty(item.Affixes);
        Assert.Equal((byte)3, item.Tail);
    }

    [Fact]
    public void Read_SealedAndCorruptedFlags_MarkFirstAffixes()
    {
        byte[] data = [2, 0, 0, 3, 0x10, 0, 0, 0, 10, 0x80 | 0x40 | 3, 0x00, 1, 1, 0x00, 2, 2, 0x00, 3, 3, 0];

        var item = ItemDataReader.Read(data);

        Assert.NotNull(item);
        Assert.True(item.IsCorrupted);
        Assert.True(item.Affixes[0].IsSealed);
        Assert.True(item.Affixes[1].IsCorrupted);
        Assert.False(item.Affixes[2].IsSealed || item.Affixes[2].IsCorrupted);
    }

    [Theory]
    [InlineData(new byte[0])]
    [InlineData(new byte[] { 3, 0, 0, 0, 0, 0, 0, 0, 0 })] // unknown version
    [InlineData(new byte[] { 2, 0, 0, 0 })] // cut short header
    [InlineData(new byte[] { 2, 0, 0, 4, 0, 1, 2, 3, 50, 2, 0x41 })] // cut short affixes
    public void Read_InvalidData_ReturnsNull(byte[] data) => Assert.Null(ItemDataReader.Read(data));

    [Fact]
    public void ReadBaseTypeId_HandlesBothVersions()
    {
        Assert.Equal(0, ItemDataReader.ReadBaseTypeId(PlainV2));
        Assert.Equal(21, ItemDataReader.ReadBaseTypeId(UniqueV6));
        Assert.Null(ItemDataReader.ReadBaseTypeId(Array.Empty<byte>()));
        Assert.Null(ItemDataReader.ReadBaseTypeId(new byte[] { 9, 1 }));
        Assert.Null(ItemDataReader.ReadBaseTypeId(new byte[] { 6, 1, 2 }));
    }

    [Fact]
    public void Write_RoundTripsLegendaryWithCorruption()
    {
        // Legendary: 2 affixes, the last one is the corruption (0x80 on a legendary).
        byte[] data = [6, 9, 9, 9, 9, 5, 0, 9, 0x10, 1, 2, 3, 0, 42, 1, 2, 3, 4, 5, 6, 7, 8, 0x80 | 2, 0x10, 5, 9, 0x20, 6, 10];

        var item = ItemDataReader.Read(data);

        Assert.NotNull(item);
        Assert.True(item.Affixes[^1].IsCorrupted);
        Assert.Equal(data, ItemDataWriter.Write(item));
    }

    [Theory]
    [MemberData(nameof(RoundTripData))]
    public void Write_ReadOutput_GivesSameBytes(byte[] data) =>
        Assert.Equal(data, ItemDataWriter.Write(ItemDataReader.Read(data)!));

    public static TheoryData<byte[]> RoundTripData => new() { PlainV2, UniqueV6 };

    [Fact]
    public void Write_UnknownVersion_Throws()
    {
        var item = new ItemData(0, 0, 0, [], null, [], 0, [], FormatVersion: 5);
        Assert.Throws<ArgumentException>(() => ItemDataWriter.Write(item));
    }

    [Fact]
    public void Write_TwoSealedAffixes_Throws()
    {
        var item = new ItemData(0, 0, 4, [0, 0, 0], null, [], 0,
            [new ItemAffixData(1, 1, 0, true), new ItemAffixData(2, 1, 0, true)]);
        Assert.Throws<ArgumentException>(() => ItemDataWriter.Write(item));
    }

    [Fact]
    public void Write_UniqueWithoutUniqueId_Throws()
    {
        var item = new ItemData(0, 0, ItemDataReader.UniqueRarity, [0, 0, 0], null, [], null, []);
        Assert.Throws<ArgumentException>(() => ItemDataWriter.Write(item));
    }

    [Fact]
    public void Write_OrdersSpecialAffixesFirstOnPlainBase()
    {
        var item = new ItemData(0, 0, 4, [0, 0, 0], null, [], 1,
        [
            new ItemAffixData(1, 1, 0, false),
            new ItemAffixData(2, 1, 0, false, IsCorrupted: true),
            new ItemAffixData(3, 1, 0, true)
        ]);

        var read = ItemDataReader.Read(ItemDataWriter.Write(item))!;

        Assert.Equal(new[] { 3, 2, 1 }, read.Affixes.Select(a => a.AffixId));
        Assert.True(read.Affixes[0].IsSealed);
        Assert.True(read.Affixes[1].IsCorrupted);
    }

    [Fact]
    public void Write_ClampsTierAndPadsImplicits()
    {
        var item = new ItemData(0, 0, 1, [7], null, [], 0, [new ItemAffixData(1, 12, 5, false)]);

        var read = ItemDataReader.Read(ItemDataWriter.Write(item))!;

        Assert.Equal(new byte[] { 7, 255, 255 }, read.ImplicitRolls);
        Assert.Equal(ItemDataWriter.MaxTier, read.Affixes[0].Tier);
    }
}
