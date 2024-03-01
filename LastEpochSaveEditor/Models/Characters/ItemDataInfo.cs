namespace LastEpochSaveEditor.Models.Characters;

public class ItemDataInfo
{
	public bool IsSeal { get; set; }
	public int UniqueOrSetId { get; set; }
	public int Patch { get; set; }
	public ItemInfoTypeEnum Type { get; set; }
	public int BaseId { get; set; }
	public QualityType Quality { get; set; }
	public int LegendaryPotencial { get; set; }
	public int Instability { get; set; }
	public int ForgingPotencial { get; set; }
	public int AffixCount { get; set; }
	public IList<int> Prefixes { get; set; } = new List<int>();
	public IList<int> UniqueModifiers { get; set; } = new List<int>();
	public IList<AffixInfo> Affixes { get; set; } = new List<AffixInfo>();
	public IList<AffixInfo> SealedAffixes { get; set; } = new List<AffixInfo>();
	public IList<AffixInfo> LegendaryAffixes { get; set; } = new List<AffixInfo>();
	public BitmapImage Icon { get; set; }
	public int Width { get; set; }
	public int Height { get; set; }

	public static readonly ItemDataInfo Empty;

	public ItemDataInfo Copy() => new()
	{
		IsSeal = IsSeal,
		UniqueOrSetId = UniqueOrSetId,
		Patch = Patch,
		Type = Empty.Type,
		BaseId = BaseId,
		LegendaryPotencial = LegendaryPotencial,
		Instability = Instability,
		ForgingPotencial = ForgingPotencial,
		AffixCount = AffixCount,
		Quality = Quality
	};

	static ItemDataInfo() => Empty = new()
	{
		IsSeal = false,
		UniqueOrSetId = -1,
		Patch = -1,
		Type = ItemInfoTypeEnum.Helmet,
		BaseId = -1,
		LegendaryPotencial = -1,
		Instability = -1,
		ForgingPotencial = -1,
		AffixCount = -1,
		Quality = QualityType.Basic
	};
}