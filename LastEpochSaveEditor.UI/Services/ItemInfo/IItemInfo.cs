namespace LastEpochSaveEditor.Services.ItemInfo;

public interface IItemInfo<TEnum>
	where TEnum : struct, Enum
{
	string BaseIcon { get; }
	int Width { get; }
	int Height { get; }
	TEnum Type { get; }
	Task<string> GetIcon(IDatabaseService db, QualityType quality, int id, ItemInfoTypeEnum itemType);
}
