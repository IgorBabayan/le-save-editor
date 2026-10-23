namespace LE.SaveEditor.Services.GameData.Items;

/// <summary>A slot of the equipment grid in the "Inventory" tab.</summary>
public enum EquipmentSlot
{
    Helmet,
    Amulet,
    Weapon,
    BodyArmour,
    OffHand,
    LeftRing,
    RightRing,
    Belt,
    Gloves,
    Boots,
    Relic
}

public static class EquipmentSlotExtensions
{
    /// <summary>
    /// <c>baseTypeID</c>s of the game's item types that fit each slot
    /// (the same ids are the second byte of an item in a save: helmet = 0, body armour = 1, wand = 10, catalyst = 19, ...).
    /// </summary>
    private static readonly IReadOnlyDictionary<EquipmentSlot, int[]> BaseTypeIds = new Dictionary<EquipmentSlot, int[]>
    {
        [EquipmentSlot.Helmet] = [0],
        [EquipmentSlot.BodyArmour] = [1],
        [EquipmentSlot.Belt] = [2],
        [EquipmentSlot.Boots] = [3],
        [EquipmentSlot.Gloves] = [4],
        // 1H axe, dagger, 1H mace, sceptre, 1H sword, wand, fist, 2H axe, 2H mace, 2H spear, 2H staff, 2H sword, bow, crossbow
        [EquipmentSlot.Weapon] = [5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 23, 24],
        // quiver, shield, catalyst
        [EquipmentSlot.OffHand] = [17, 18, 19],
        [EquipmentSlot.Amulet] = [20],
        [EquipmentSlot.LeftRing] = [21],
        [EquipmentSlot.RightRing] = [21],
        [EquipmentSlot.Relic] = [22]
    };

    /// <summary>
    /// Words of item type names that fit each slot; used only when an item type in <c>base.db</c> has no id.
    /// </summary>
    private static readonly IReadOnlyDictionary<EquipmentSlot, string[]> TypeNameWords = new Dictionary<EquipmentSlot, string[]>
    {
        [EquipmentSlot.Helmet] = ["helmet", "helm"],
        [EquipmentSlot.BodyArmour] = ["bodyarmour", "bodyarmor", "chest"],
        [EquipmentSlot.Belt] = ["belt"],
        [EquipmentSlot.Boots] = ["boots"],
        [EquipmentSlot.Gloves] = ["gloves"],
        [EquipmentSlot.Weapon] = ["axe", "dagger", "mace", "sceptre", "sword", "wand", "fist", "spear", "staff", "bow", "crossbow"],
        [EquipmentSlot.OffHand] = ["quiver", "shield", "catalyst"],
        [EquipmentSlot.Amulet] = ["amulet"],
        [EquipmentSlot.LeftRing] = ["ring"],
        [EquipmentSlot.RightRing] = ["ring"],
        [EquipmentSlot.Relic] = ["relic"]
    };

    /// <summary><c>containerID</c> of each equipment slot in <c>savedItems</c> of a character save.</summary>
    private static readonly IReadOnlyDictionary<int, EquipmentSlot> SlotsByContainerId = new Dictionary<int, EquipmentSlot>
    {
        [2] = EquipmentSlot.Helmet,
        [3] = EquipmentSlot.BodyArmour,
        [4] = EquipmentSlot.Weapon,
        [5] = EquipmentSlot.OffHand,
        [6] = EquipmentSlot.Gloves,
        [7] = EquipmentSlot.Belt,
        [8] = EquipmentSlot.Boots,
        [9] = EquipmentSlot.LeftRing,
        [10] = EquipmentSlot.RightRing,
        [11] = EquipmentSlot.Amulet,
        [12] = EquipmentSlot.Relic
    };

    /// <summary>The equipment slot a saved item with <paramref name="containerId"/> is in; <c>null</c> for any other container.</summary>
    public static EquipmentSlot? FromContainerId(int containerId) =>
        SlotsByContainerId.TryGetValue(containerId, out var slot) ? slot : null;

    /// <summary><c>containerID</c> of <paramref name="slot"/> in <c>savedItems</c> (inverse of <see cref="FromContainerId"/>).</summary>
    public static int ToContainerId(EquipmentSlot slot) =>
        SlotsByContainerId.First(pair => pair.Value == slot).Key;

    /// <summary>
    /// The first equipment slot an item of type <paramref name="baseTypeId"/> fits (rings → left ring);
    /// <c>null</c> for items that are not equipment (idols, keys, ...).
    /// </summary>
    public static EquipmentSlot? FromBaseTypeId(int baseTypeId)
    {
        foreach (var slot in Enum.GetValues<EquipmentSlot>())
        {
            if (BaseTypeIds[slot].Contains(baseTypeId))
                return slot;
        }

        return null;
    }

    extension(EquipmentSlot slot)
    {
        /// <summary>Label shown on the slot and in the item builder title.</summary>
        public string DisplayName => slot switch
        {
            EquipmentSlot.BodyArmour => "Body armour",
            EquipmentSlot.OffHand => "Off-hand",
            EquipmentSlot.LeftRing or EquipmentSlot.RightRing => "Ring",
            _ => slot.ToString()
        };

        public bool AcceptsBaseType(int baseTypeId) => BaseTypeIds[slot].Contains(baseTypeId);

        /// <summary>Fallback for item types without an id: matches the normalized type name.</summary>
        public bool AcceptsTypeName(string normalizedTypeName) =>
            TypeNameWords[slot].Any(word => slot is EquipmentSlot.LeftRing or EquipmentSlot.RightRing
                ? normalizedTypeName == word
                : normalizedTypeName.Contains(word, StringComparison.Ordinal));
    }
}
