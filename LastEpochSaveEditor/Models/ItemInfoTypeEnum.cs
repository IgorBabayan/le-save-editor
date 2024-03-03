namespace LastEpochSaveEditor.Models;

public enum ItemInfoTypeEnum
{
	[Description(Const.ARMOURS_HELMETS)]
    Helmet = 0,

	[Description(Const.ARMOURS_BODY)]
    Body = 1,

	[Description(Const.ARMOURS_BELTS)]
    Belt = 2,

	[Description(Const.ARMOURS_BOOTS)]
    Boots = 3,

	[Description(Const.ARMOURS_GLOVES)]
    Gloves = 4,

	[Description(Const.ONE_HAND_AXES)]
    OneHandAxes = 5,

	[Description(Const.ONE_HAND_DAGGERS)]
    OneHandDaggers = 6,

	[Description(Const.ONE_HAND_MACES)]
	OneHandMaces = 7,

	[Description(Const.ONE_HAND_SCEPTERS)]
	OneHandScepter = 8,

	[Description(Const.ONE_HAND_SWORDS)]
	OneHandSwords = 9,

	[Description(Const.ONE_HAND_WANDS)]
	Wands = 10,

	[Description(Const.ONE_HAND_WANDS)]
	OneHandFist = 11,

	[Description(Const.TWO_HAND_AXES)]
	TwoHandAxes = 12,

	[Description(Const.TWO_HAND_MACES)]
	TwoHandMaces = 13,

	[Description(Const.TWO_HAND_POLEARM)]
	TwoHandPolearm = 14,

	[Description(Const.TWO_HAND_STAFF)]
	Staff = 15,

	[Description(Const.TWO_HAND_SWORDS)]
	TwoHandSwords = 16,

	[Description(Const.QUIVER)]
	Quiver = 17,

	[Description(Const.SHIELD)]
	Shield = 18,

	[Description(Const.CATALYST)]
	Catalyst = 19,

	[Description(Const.ACCESSORIES_AMULET)]
	Amulet = 20,

	[Description(Const.ACCESSORIES_RING)]
	Ring = 21,

	[Description(Const.ACCESSORIES_RELIC)]
    Relic = 22,

	[Description(Const.TWO_HAND_BOWS)]
	Bows = 23,

	[Description(Const.TWO_HAND_BOWS)]
	Crossbow = 24,

	[Description(Const.SMALL_IDOL)]
	SmallIdol = 25,

	[Description(Const.SMALL_LAGONIAN_IDOL)]
	SmallLagonianIdol = 26,

	[Description(Const.HUMBLE_IDOL)]
	HumbleIdol = 27,

	[Description(Const.STOUT_IDOL)]
	StoutIdol = 28,

	[Description(Const.GRAND_IDOL)]
	GrandIdol = 29,

	[Description(Const.LARGE_IDOL)]
	LargeIdol = 30,

	[Description(Const.ORNATE_IDOL)]
	OrnateIdol = 31,
	HugeIdol = 32,

	[Description(Const.ADORNED_IDOL)]
	AdornedIdol = 33,
	Blessing = 34,
	GreaterLens = 35,
	ArctusLens = 36,
	MesembriaLens = 37,
	EosLens = 38,
	DysisLens = 39,
	AffixShard = 101,
	Rune = 102,
	Glyph = 103,
	Key = 104,
	LostMemory = 105,
	Resonance = 106,

	/// <summary>
	/// <list type="bullet">
	///		<item>
	///			<see cref="OneHandAxes"/>
	///		</item>
	///		<item>
	///			<see cref="OneHandDaggers"/>
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
	[Description(Const.ONE_HAND_WEAPON)]
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
	[Description(Const.TWO_HAND_WEAPON)]
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
	[Description(Const.ACCESSORIES)]
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
	[Description(Const.ARMOURS)]
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
	[Description(Const.OFF_HANDS)]
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
	[Description(Const.IDOLS)]
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