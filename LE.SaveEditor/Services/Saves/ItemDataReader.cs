using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

/// <summary>
/// Decodes the <c>data</c> bytes of a saved equipment item (format versions 2 and 6);
/// <see cref="ItemDataWriter"/> encodes them back.
/// </summary>
/// <remarks>
/// Version 6 is version 2 with a 4-byte item instance id right after the version byte:
/// <code>
/// v2: [0] 2   [1] baseTypeID   [2] subTypeID   [3] rarity   [4] flags   [5..7] implicit rolls   ...
/// v6: [0] 6   [1..4] instance id   [5] baseTypeID   [6] subTypeID   [7] rarity   [8] flags   [9..11] implicit rolls   ...
/// </code>
/// After the header both versions are the same. Layout with the offsets of version 2:
/// <code>
/// [0] format version   [1] baseTypeID   [2] subTypeID   [3] rarity   [4] flags   [5..7] implicit rolls
/// plain base:              [8] forging potential   [9] affix count   affixes...   [last] 0
/// unique / set:            [8..9] uniqueID (big endian)   [10..17] unique modifier rolls   [18] legendary potential
/// legendary:               [8..9] uniqueID   [10..17] unique modifier rolls   [18] affix count   affixes...   [last] 0
/// </code>
/// The affix count keeps the number in its low bits; bit 7 marks a sealed affix, which is stored first.
/// An affix is 3 bytes: <c>tier &lt;&lt; 4 | affixId &gt;&gt; 8</c>, <c>affixId &amp; 0xFF</c>, roll.
/// </remarks>
public static class ItemDataReader
{
    /// <summary>Format of the editor's first saves: no instance id.</summary>
    public const int Version2 = 2;

    /// <summary>Current game format: a 4-byte instance id follows the version byte.</summary>
    public const int Version6 = 6;

    internal const int InstanceIdLength = 4;

    public const int UniqueRarity = 7;
    public const int SetRarity = 8;
    public const int LegendaryRarity = 9;

    internal const int ImplicitCount = 3;
    internal const int UniqueRollCount = 8;
    internal const int SealedFlag = 0x80;
    internal const int AffixCountMask = 0x0F;

    /// <summary>
    /// Only the item type (<c>baseTypeID</c>) of <paramref name="data"/>, for any item (equipment, idol, key, ...);
    /// <c>null</c> when the format is unknown or the data is cut short.
    /// </summary>
    public static int? ReadBaseTypeId(IReadOnlyList<byte> data)
    {
        if (data.Count == 0)
            return null;

        var position = data[0] switch
        {
            Version2 => 1,
            Version6 => 1 + InstanceIdLength,
            _ => -1
        };
        return position >= 0 && position < data.Count ? data[position] : null;
    }

    /// <summary>Decodes <paramref name="data"/>; <c>null</c> when it is not an equipment item in a known format.</summary>
    public static ItemData? Read(IReadOnlyList<byte> data)
    {
        if (data.Count == 0)
            return null;

        // Version (+ instance id), type, sub type, rarity, flags and the implicit rolls come first in every format.
        var version = data[0];
        var start = version switch
        {
            Version2 => 1,
            Version6 => 1 + InstanceIdLength,
            _ => -1
        };
        if (start < 0 || data.Count < start + 4 + ImplicitCount + 1)
            return null;

        // Kept as stored, so the item can be written back unchanged.
        var instanceId = start > 1 ? Slice(data, 1, InstanceIdLength) : [];

        var baseTypeId = data[start];
        var subTypeId = data[start + 1];
        var rarity = data[start + 2];
        var flags = data[start + 3];
        var implicitRolls = Slice(data, start + 4, ImplicitCount);
        var position = start + 4 + ImplicitCount;

        if (rarity is UniqueRarity or SetRarity or LegendaryRarity)
        {
            if (data.Count < position + 2 + UniqueRollCount)
                return null;

            var uniqueId = (data[position] << 8) | data[position + 1];
            position += 2;
            var uniqueRolls = Slice(data, position, UniqueRollCount);
            position += UniqueRollCount;

            // Uniques and set items end with their legendary potential; only legendaries carry affixes.
            IReadOnlyList<ItemAffixData>? affixes = rarity == LegendaryRarity ? ReadAffixes(data, ref position) : [];
            return affixes is null
                ? null
                : new ItemData(baseTypeId, subTypeId, rarity, implicitRolls, uniqueId, uniqueRolls, null, affixes, version,
                    instanceId, flags, TailAt(data, position));
        }

        var forgingPotential = data[position++];
        var plainAffixes = ReadAffixes(data, ref position);
        return plainAffixes is null
            ? null
            : new ItemData(baseTypeId, subTypeId, rarity, implicitRolls, null, [], forgingPotential, plainAffixes, version,
                instanceId, flags, TailAt(data, position));
    }

    private static byte? TailAt(IReadOnlyList<byte> data, int position) => position < data.Count ? data[position] : null;

    /// <summary>
    /// Affix count followed by the affixes; <c>null</c> when the data is cut short.
    /// <paramref name="position"/> is moved past the affixes.
    /// </summary>
    private static IReadOnlyList<ItemAffixData>? ReadAffixes(IReadOnlyList<byte> data, ref int position)
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
