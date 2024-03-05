namespace LastEpochSaveEditor.Models;

public enum ItemInfoTypeEnum
{
	[Description(ARMOURS_HELMETS)]
    Helmet = 0,

	[Description(ARMOURS_BODY)]
    Body = 1,

	[Description(ARMOURS_BELTS)]
    Belt = 2,

	[Description(ARMOURS_BOOTS)]
    Boots = 3,

	[Description(ARMOURS_GLOVES)]
    Gloves = 4,

	[Description(ONE_HAND_AXES)]
    OneHandAxes = 5,

	[Description(ONE_HAND_DAGGERS)]
    Daggers = 6,

	[Description(ONE_HAND_MACES)]
	OneHandMaces = 7,

	[Description(ONE_HAND_SCEPTERS)]
	OneHandScepter = 8,

	[Description(ONE_HAND_SWORDS)]
	OneHandSwords = 9,

	[Description(ONE_HAND_WANDS)]
	Wands = 10,

	[Description(ONE_HAND_WANDS)]
	OneHandFist = 11,

	[Description(TWO_HAND_AXES)]
	TwoHandAxes = 12,

	[Description(TWO_HAND_MACES)]
	TwoHandMaces = 13,

	[Description(TWO_HAND_POLEARM)]
	TwoHandPolearm = 14,

	[Description(TWO_HAND_STAFF)]
	Staff = 15,

	[Description(TWO_HAND_SWORDS)]
	TwoHandSwords = 16,

	[Description(QUIVER)]
	Quiver = 17,

	[Description(SHIELD)]
	Shield = 18,

	[Description(CATALYST)]
	Catalyst = 19,

	[Description(ACCESSORIES_AMULET)]
	Amulet = 20,

	[Description(ACCESSORIES_RING)]
	Ring = 21,

	[Description(ACCESSORIES_RELIC)]
    Relic = 22,

	[Description(TWO_HAND_BOWS)]
	Bows = 23,

	[Description(TWO_HAND_BOWS)]
	Crossbow = 24,

	[Description(SMALL_IDOL)]
	SmallIdol = 25,

	[Description(SMALL_LAGONIAN_IDOL)]
	SmallLagonianIdol = 26,

	[Description(HUMBLE_IDOL)]
	HumbleIdol = 27,

	[Description(STOUT_IDOL)]
	StoutIdol = 28,

	[Description(GRAND_IDOL)]
	GrandIdol = 29,

	[Description(LARGE_IDOL)]
	LargeIdol = 30,

	[Description(ORNATE_IDOL)]
	OrnateIdol = 31,

	[Description(HUGE_IDOL)]
	HugeIdol = 32,

	[Description(ADORNED_IDOL)]
	AdornedIdol = 33,

	[Description(BLESSING)]
	Blessing = 34,

	[Description(GREATER_LENS)]
	GreaterLens = 35,

	[Description(ARCTUS_LENS)]
	ArctusLens = 36,

	[Description(MESEMBRIA_LENS)]
	MesembriaLens = 37,

	[Description(EOS_LENS)]
	EosLens = 38,

	[Description(DYSIS_LENS)]
	DysisLens = 39,

	[Description(AFFIX_SHARD)]
	AffixShard = 101,

	[Description(RUNE)]
	Rune = 102,

	[Description(GLYPH)]
	Glyph = 103,

	[Description(KEY)]
	Key = 104,

	[Description(LOST_MEMORY)]
	LostMemory = 105,

	[Description(RESONANCE)]
	Resonance = 106,

	/// <summary>
	/// <list type="bullet">
	///		<item>
	///			<see cref="OneHandAxes"/>
	///		</item>
	///		<item>
	///			<see cref="Daggers"/>
	///		</item>
	///		<item>
	///			<see cref="OneHandMaces"/>
	///		</item>
	///		<item>
	///			<see cref="OneHandScepter"/>
	///		</item>
	///		<item>
	///			<see cref="OneHandSwords"/>
	///		</item>
	///		<item>
	///			<see cref="OneHandFist"/>
	///		</item>
	///		<item>
	///			<see cref="Wands"/>
	///		</item>
	/// </list>
	/// </summary>
	[Description(ONE_HAND_WEAPON)]
	OneHandWeapons = 9000,

	/// <summary>
	/// <list type="bullet">
	///		<item>
	///			<see cref="TwoHandAxes"/>
	///		</item>
	///		<item>
	///			<see cref="TwoHandMaces"/>
	///		</item>
	///		<item>
	///			<see cref="TwoHandSwords"/>
	///		</item>
	///		<item>
	///			<see cref="TwoHandPolearm"/>
	///		</item>
	///		<item>
	///			<see cref="Staff"/>
	///		</item>
	/// </list>
	/// </summary>
	[Description(TWO_HAND_WEAPON)]
	TwoHandWeapons = 9001,

	/// <summary>
	/// <list type="bullet">
	///		<item>
	///			<see cref="OneHandWeapons"/>
	///		</item>
	///		<item>
	///			<see cref="TwoHandWeapons"/>
	///		</item>
	///		<item>
	///			<see cref="Bows"/>
	///		</item>
	///		<item>
	///			<see cref="Crossbow"/>
	///		</item>
	/// </list>
	/// </summary>
	Weapons = 9002,

	/// <summary>
	/// <list type="bullet">
	///		<item>
	///			<see cref="Amulet"/>
	///		</item>
	///		<item>
	///			<see cref="Ring"/>
	///		</item>
	///		<item>
	///			<see cref="Relic"/>
	///		</item>
	/// </list>
	/// </summary>
	[Description(ACCESSORIES)]
	Accessories = 9003,

	/// <summary>
	/// <list type="bullet">
	///		<item>
	///			<see cref="Helmet"/>
	///		</item>
	///		<item>
	///			<see cref="Body"/>
	///		</item>
	///		<item>
	///			<see cref="Belt"/>
	///		</item>
	///		<item>
	///			<see cref="Boots"/>
	///		</item>
	///		<item>
	///			<see cref="Gloves"/>
	///		</item>
	/// </list>
	/// </summary>
	[Description(ARMOURS)]
	Armours = 9004,

	/// <summary>
	/// <list type="bullet">
	///		<item>
	///			<see cref="Quiver"/>
	///		</item>
	///		<item>
	///			<see cref="Shield"/>
	///		</item>
	///		<item>
	///			<see cref="Catalyst"/>
	///		</item>
	/// </list>
	/// </summary>
	[Description(OFF_HANDS)]
	OffHands = 9005,

	/// <summary>
	/// <list type="bullet">
	///		<item>
	///			<see cref="SmallIdol"/>
	///		</item>
	///		<item>
	///			<see cref="SmallLagonianIdol"/>
	///		</item>
	///		<item>
	///			<see cref="HumbleIdol"/>
	///		</item>
	///		<item>
	///			<see cref="StoutIdol"/>
	///		</item>
	///		<item>
	///			<see cref="GrandIdol"/>
	///		</item>
	///		<item>
	///			<see cref="LargeIdol"/>
	///		</item>
	///		<item>
	///			<see cref="OrnateIdol"/>
	///		</item>
	///		<item>
	///			<see cref="HugeIdol"/>
	///		</item>
	///		<item>
	///			<see cref="AdornedIdol"/>
	///		</item>
	/// </list>
	/// </summary>
	[Description(IDOLS)]
	Idols = 9006,

	/// <summary>
	/// <list type="bullet">
	///		<item>
	///			<see cref="GreaterLens"/>
	///		</item>
	///		<item>
	///			<see cref="ArctusLens"/>
	///		</item>
	///		<item>
	///			<see cref="MesembriaLens"/>
	///		</item>
	///		<item>
	///			<see cref="EosLens"/>
	///		</item>
	///		<item>
	///			<see cref="DysisLens"/>
	///		</item>
	/// </list>
	/// </summary>
	Lens = 9007,

	/// <summary>
	/// <list type="bullet">
	///		<item>
	///			<see cref="Blessing"/>
	///		</item>
	///		<item>
	///			<see cref="Rune"/>
	///		</item>
	///		<item>
	///			<see cref="Glyph"/>
	///		</item>
	///		<item>
	///			<see cref="Key"/>
	///		</item>
	///		<item>
	///			<see cref="LostMemory"/>
	///		</item>
	///		<item>
	///			<see cref="Resonance"/>
	///		</item>
	/// </list>
	/// </summary>
	Misc = 9008,

	/// <summary>
	/// <list type="bullet">
	///		<item>
	///			<see cref="Armours"/>
	///		</item>
	///		<item>
	///			<see cref="Weapons"/>
	///		</item>
	///		<item>
	///			<see cref="Accessories"/>
	///		</item>
	///		<item>
	///			<see cref="OffHands"/>
	///		</item>
	///		<item>
	///			<see cref="Idols"/>
	///		</item>
	///		<item>
	///			<see cref="Lens"/>
	///		</item>
	///		<item>
	///			<see cref="AffixShard"/>
	///		</item>
	///		<item>
	///			<see cref="Rune"/>
	///		</item>
	///		<item>
	///			<see cref="Glyph"/>
	///		</item>
	///		<item>
	///			<see cref="Key"/>
	///		</item>
	///		<item>
	///			<see cref="LostMemory"/>
	///		</item>
	///		<item>
	///			<see cref="Resonance"/>
	///		</item>
	/// </list>
	/// </summary>
	All = int.MaxValue,

	None = -1
}