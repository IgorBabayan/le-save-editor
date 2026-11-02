using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Services.Saves;

/// <summary>
/// Decodes and encodes the <c>data</c> bytes of a crafting material in the backpack: a rune, a glyph or an affix shard.
/// </summary>
/// <remarks>
/// A crafting material has no rarity, rolls or affixes; its bytes are the header of every item followed by the sub type:
/// <code>
/// v2: [0] 2   [1] baseTypeID   [2] subTypeID                     e.g. [2, 102, 2] = Rune of Removal
/// v6: [0] 6   [1..4] instance id   [5] baseTypeID   [6] subTypeID
/// </code>
/// The sub type of an affix shard is the <c>affixId</c> of its affix, which goes up to 1156 and does not fit one byte.
/// Shards are written with a 2-byte big-endian sub type (as the format stores unique ids): <c>[2, 101, id &gt;&gt; 8,
/// id &amp; 0xFF]</c>; shards with a 1-byte sub type are read too. Bytes after the sub type of a material read from
/// the save are kept when it is written back.
/// </remarks>
public static class CraftingMaterialData
{
    /// <summary>A decoded crafting material: its kind and sub type.</summary>
    public readonly record struct Value(CraftingMaterialKind Kind, int SubTypeId);

    /// <summary>
    /// The crafting material stored in <paramref name="data"/>; <c>null</c> when it is another item or the format is
    /// unknown.
    /// </summary>
    public static Value? Read(IReadOnlyList<byte> data)
    {
        if (HeaderLength(data) is not { } header
            || data.Count <= header + 1
            || CraftingMaterialKindExtensions.FromBaseTypeId(data[header]) is not { } kind)
            return null;

        var subTypeStart = header + 1;
        var subTypeId = kind == CraftingMaterialKind.AffixShard && data.Count >= subTypeStart + 2
            ? (data[subTypeStart] << 8) | data[subTypeStart + 1]
            : data[subTypeStart];
        return new Value(kind, subTypeId);
    }

    /// <summary>
    /// The bytes of <paramref name="material"/>. With <paramref name="original"/> (the bytes of the item it replaces)
    /// the format version, the instance id and any bytes after the sub type are kept; a new material is written in
    /// format 2.
    /// </summary>
    /// <exception cref="ArgumentException">The sub type does not fit the bytes the material kind stores it in.</exception>
    public static byte[] Write(CraftingMaterial material, IReadOnlyList<byte>? original = null)
    {
        var shard = material.Kind == CraftingMaterialKind.AffixShard;
        if (material.SubTypeId < 0 || material.SubTypeId > (shard ? ushort.MaxValue : byte.MaxValue))
            throw new ArgumentException($"{material.FullName} has a sub type ({material.SubTypeId}) that cannot be stored.", nameof(material));

        var result = new List<byte>();
        var header = original is not null ? HeaderLength(original) : null;
        if (original is not null && header is { } length)
        {
            for (var i = 0; i < length; i++)
                result.Add(original[i]);
        }
        else
        {
            result.Add(ItemDataReader.Version2);
        }

        result.Add((byte)material.BaseTypeId);
        if (shard)
            result.Add((byte)(material.SubTypeId >> 8));
        result.Add((byte)(material.SubTypeId & 0xFF));

        // Bytes the game wrote after the sub type of the replaced material (none are known) stay as they were.
        if (original is not null && header is { } start && Read(original) is { } old)
        {
            var oldEnd = start + 1 + (old.Kind == CraftingMaterialKind.AffixShard && original.Count >= start + 3 ? 2 : 1);
            for (var i = oldEnd; i < original.Count; i++)
                result.Add(original[i]);
        }

        return result.ToArray();
    }

    /// <summary>Length of the version byte (and instance id of format 6); <c>null</c> for an unknown format.</summary>
    private static int? HeaderLength(IReadOnlyList<byte> data) =>
        data.Count == 0
            ? null
            : data[0] switch
            {
                ItemDataReader.Version2 => 1,
                ItemDataReader.Version6 => 1 + ItemDataReader.InstanceIdLength,
                _ => null
            };
}
