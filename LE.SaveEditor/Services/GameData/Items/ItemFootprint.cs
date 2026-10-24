namespace LE.SaveEditor.Services.GameData.Items;

/// <summary>
/// Number of cells an item takes in a grid container (backpack, stash), <see cref="Width"/> columns ×
/// <see cref="Height"/> rows. The size depends only on the item type (<c>baseTypeID</c>).
/// </summary>
public readonly record struct ItemFootprint(int Width, int Height)
{
    /// <summary>Size of items whose type is not listed (keys, shards, unknown types).</summary>
    public static readonly ItemFootprint Single = new(1, 1);

    /// <summary>
    /// Size of each item type by <c>baseTypeID</c>. <c>base.db</c> does not carry item sizes, so they are kept here;
    /// adjust an entry if an item type looks wrong in the backpack.
    /// </summary>
    private static readonly IReadOnlyDictionary<int, ItemFootprint> ByBaseTypeId = new Dictionary<int, ItemFootprint>
    {
        [0] = new(2, 2),  // helmet
        [1] = new(2, 3),  // body armour
        [2] = new(2, 1),  // belt
        [3] = new(2, 2),  // boots
        [4] = new(2, 2),  // gloves
        [5] = new(1, 3),  // one-handed axe
        [6] = new(1, 3),  // dagger
        [7] = new(1, 3),  // one-handed mace
        [8] = new(1, 3),  // sceptre
        [9] = new(1, 3),  // one-handed sword
        [10] = new(1, 3), // wand
        [11] = new(1, 2), // fist
        [12] = new(2, 4), // two-handed axe
        [13] = new(2, 4), // two-handed mace
        [14] = new(1, 4), // two-handed spear
        [15] = new(1, 4), // two-handed staff
        [16] = new(2, 4), // two-handed sword
        [17] = new(2, 3), // quiver
        [18] = new(2, 2), // shield
        [19] = new(1, 2), // catalyst
        [20] = new(1, 1), // amulet
        [21] = new(1, 1), // ring
        [22] = new(2, 2), // relic
        [23] = new(2, 4), // bow
        [24] = new(2, 4), // crossbow
        [25] = new(1, 1), // small idol
        [26] = new(1, 1), // minor idol
        [27] = new(2, 1), // humble idol
        [28] = new(1, 2), // stout idol
        [29] = new(3, 1), // grand idol
        [30] = new(1, 3), // large idol
        [31] = new(4, 1), // ornate idol
        [32] = new(1, 4), // huge idol
        [33] = new(2, 2)  // adorned idol
    };

    /// <summary>Size of an item of type <paramref name="baseTypeId"/>; <see cref="Single"/> for unlisted types.</summary>
    public static ItemFootprint Of(int baseTypeId) =>
        ByBaseTypeId.TryGetValue(baseTypeId, out var footprint) ? footprint : Single;
}
