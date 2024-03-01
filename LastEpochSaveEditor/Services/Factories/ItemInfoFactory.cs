namespace LastEpochSaveEditor.Services.Factories;

public interface IItemInfoFactory
{
	IItemInfo<ItemInfoTypeEnum> Create(ItemInfoTypeEnum type);
}

public class ItemInfoFactory : IItemInfoFactory
{
	private readonly Func<IEnumerable<IItemInfo<ItemInfoTypeEnum>>> _factory;

	public IItemInfo<ItemInfoTypeEnum> Create(ItemInfoTypeEnum type)
	{
		var set = _factory();
		var result = set.First(x => x.Type == type);
		return result!;
	}

	public ItemInfoFactory(Func<IEnumerable<IItemInfo<ItemInfoTypeEnum>>> factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));
}
