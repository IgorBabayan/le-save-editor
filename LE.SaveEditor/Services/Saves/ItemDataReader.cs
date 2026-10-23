using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

/// <summary>
/// Decodes the <c>data</c> bytes of a saved equipment item (format version 2).
/// </summary>
/// <remarks>
/// Layout:
/// <code>
/// [0] format version (2)   [1] baseTypeID   [2] subTypeID   [3] rarity   [4] flags   [5..7] implicit rolls
/// plain base:              [8] forging potential   [9] affix count   affixes...   [last] 0
/// unique / set:            [8..9] uniqueID (big endian)   [10..17] unique modifier rolls   [18] legendary potential
/// legendary:               [8..9] uniqueID   [10..17] unique modifier rolls   [18] affix count   affixes...   [last] 0
/// </code>
/// The affix count keeps the number in its low bits; bit 7 marks a sealed affix, which is stored first.
/// An affix is 3 bytes: <c>tier &lt;&lt; 4 | affixId &gt;&gt; 8</c>, <c>affixId &amp; 0xFF</c>, roll.
/// </remarks>
public static class ItemDataReader
{
    public const int SupportedVersion = 2;

    public const int UniqueRarity = 7;
    public const int SetRarity = 8;
    public const int LegendaryRarity = 9;

    private const int ImplicitCount = 3;
    private const int UniqueRollCount = 8;
    private const int SealedFlag = 0x80;
    private const int AffixCountMask = 0x0F;

    /// <summary>Decodes <paramref name="data"/>; <c>null</c> when it is not an equipment item in a known format.</summary>
    public static ItemData? Read(IReadOnlyList<byte> data)
    {
        // Version, type, sub type, rarity, flags and the implicit rolls come first in every format.
        const int headerLength = 5 + ImplicitCount;
        if (data.Count < headerLength + 1 || data[0] != SupportedVersion)
            return null;

        var baseTypeId = data[1];
        var subTypeId = data[2];
        var rarity = data[3];
        var implicitRolls = Slice(data, 5, ImplicitCount);
        var position = headerLength;

        if (rarity is UniqueRarity or SetRarity or LegendaryRarity)
        {
            if (data.Count < position + 2 + UniqueRollCount)
                return null;

            var uniqueId = (data[position] << 8) | data[position + 1];
            position += 2;
            var uniqueRolls = Slice(data, position, UniqueRollCount);
            position += UniqueRollCount;

            // Uniques and set items end with their legendary potential; only legendaries carry affixes.
            IReadOnlyList<ItemAffixData>? affixes = rarity == LegendaryRarity ? ReadAffixes(data, position) : [];
            return affixes is null
                ? null
                : new ItemData(baseTypeId, subTypeId, rarity, implicitRolls, uniqueId, uniqueRolls, null, affixes);
        }

        var forgingPotential = data[position++];
        var plainAffixes = ReadAffixes(data, position);
        return plainAffixes is null
            ? null
            : new ItemData(baseTypeId, subTypeId, rarity, implicitRolls, null, [], forgingPotential, plainAffixes);
    }

    /// <summary>Affix count followed by the affixes; <c>null</c> when the data is cut short.</summary>
    private static IReadOnlyList<ItemAffixData>? ReadAffixes(IReadOnlyList<byte> data, int position)
    {
        if (data.Count <= position)
            return null;

        var countByte = data[position++];
        var count = countByte & AffixCountMask;
        var hasSealed = (countByte & SealedFlag) != 0;
        if (data.Count < position + count * 3)
            return null;

        var affixes = new List<ItemAffixData>(count);
        for (var i = 0; i < count; i++, position += 3)
        {
            var head = data[position];
            var affixId = ((head & 0x0F) << 8) | data[position + 1];
            var tier = ((head >> 4) & 0x07) + 1;
            affixes.Add(new ItemAffixData(affixId, tier, data[position + 2], IsSealed: hasSealed && i == 0));
        }

        return affixes;
    }

    private static byte[] Slice(IReadOnlyList<byte> data, int start, int length)
    {
        var result = new byte[length];
        for (var i = 0; i < length; i++)
            result[i] = data[start + i];
        return result;
    }
}
