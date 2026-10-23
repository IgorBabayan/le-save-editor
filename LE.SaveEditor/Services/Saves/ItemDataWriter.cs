using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

/// <summary>
/// Encodes an equipment item into the <c>data</c> bytes of a save: the inverse of <see cref="ItemDataReader"/>,
/// whose remarks describe the layout. <c>Write(Read(data))</c> gives back <c>data</c> for every item the reader decodes.
/// </summary>
public static class ItemDataWriter
{
    /// <summary>Highest tier the 3 tier bits of an affix can hold (T8).</summary>
    public const int MaxTier = 8;

    /// <summary>Highest affix id the 12 id bits of an affix can hold.</summary>
    public const int MaxAffixId = 0xFFF;

    /// <summary>Encodes <paramref name="item"/>.</summary>
    /// <exception cref="ArgumentException">The item cannot be stored (unknown format version, too many affixes, ...).</exception>
    public static byte[] Write(ItemData item)
    {
        var bytes = new List<byte>(32) { (byte)item.FormatVersion };

        switch (item.FormatVersion)
        {
            case ItemDataReader.Version2:
                break;
            case ItemDataReader.Version6:
                AddPadded(bytes, item.InstanceId ?? [], ItemDataReader.InstanceIdLength, 0);
                break;
            default:
                throw new ArgumentException($"Item format version {item.FormatVersion} cannot be written.", nameof(item));
        }

        bytes.Add(ToByte(item.BaseTypeId, nameof(item.BaseTypeId)));
        bytes.Add(ToByte(item.SubTypeId, nameof(item.SubTypeId)));
        bytes.Add(ToByte(item.RarityCode, nameof(item.RarityCode)));
        bytes.Add(ToByte(item.Flags, nameof(item.Flags)));
        AddPadded(bytes, item.ImplicitRolls, ItemDataReader.ImplicitCount, byte.MaxValue);

        if (item.RarityCode is ItemDataReader.UniqueRarity or ItemDataReader.SetRarity or ItemDataReader.LegendaryRarity)
        {
            if (item.UniqueId is not { } uniqueId || uniqueId is < 0 or > ushort.MaxValue)
                throw new ArgumentException("A unique, set or legendary item needs a unique id (0 … 65535).", nameof(item));

            bytes.Add((byte)(uniqueId >> 8));
            bytes.Add((byte)(uniqueId & 0xFF));
            AddPadded(bytes, item.UniqueRolls, ItemDataReader.UniqueRollCount, 0);

            // Uniques and set items end with their legendary potential; legendaries carry affixes and a closing 0.
            if (item.RarityCode == ItemDataReader.LegendaryRarity)
                AddAffixes(bytes, item.Affixes);
        }
        else
        {
            bytes.Add(ToByte(item.ForgingPotential ?? 0, nameof(item.ForgingPotential)));
            AddAffixes(bytes, item.Affixes);
        }

        bytes.Add(item.Tail ?? 0);
        return [.. bytes];
    }

    /// <summary>Affix count (bit 7 = the first affix is sealed) followed by 3 bytes per affix, the sealed one first.</summary>
    private static void AddAffixes(List<byte> bytes, IReadOnlyList<ItemAffixData> affixes)
    {
        var sealedAffixes = affixes.Where(a => a.IsSealed).ToList();
        if (sealedAffixes.Count > 1)
            throw new ArgumentException("An item can have only one sealed affix.", nameof(affixes));
        if (affixes.Count > ItemDataReader.AffixCountMask)
            throw new ArgumentException($"An item can have at most {ItemDataReader.AffixCountMask} affixes.", nameof(affixes));

        var ordered = sealedAffixes.Concat(affixes.Where(a => !a.IsSealed)).ToList();
        bytes.Add((byte)(ordered.Count | (sealedAffixes.Count > 0 ? ItemDataReader.SealedFlag : 0)));

        foreach (var affix in ordered)
        {
            if (affix.AffixId is < 0 or > MaxAffixId)
                throw new ArgumentException($"Affix id {affix.AffixId} cannot be stored.", nameof(affixes));

            var tierBits = Math.Clamp(affix.Tier, 1, MaxTier) - 1;
            bytes.Add((byte)((tierBits << 4) | (affix.AffixId >> 8)));
            bytes.Add((byte)(affix.AffixId & 0xFF));
            bytes.Add(affix.Roll);
        }
    }

    /// <summary>Exactly <paramref name="length"/> bytes of <paramref name="values"/>, filled up with <paramref name="fill"/>.</summary>
    private static void AddPadded(List<byte> bytes, IReadOnlyList<byte> values, int length, byte fill)
    {
        for (var i = 0; i < length; i++)
            bytes.Add(i < values.Count ? values[i] : fill);
    }

    private static byte ToByte(int value, string name) =>
        value is >= byte.MinValue and <= byte.MaxValue
            ? (byte)value
            : throw new ArgumentException($"{name} {value} does not fit in one byte.", name);
}
