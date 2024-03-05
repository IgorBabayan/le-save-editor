namespace LastEpochSaveEditor.Factories;

public interface ISubItemRepositoryFactory : IRepositoryFactory<SubItem> { }

public class SubItemRepositoryFactory : ISubItemRepositoryFactory
{
	private readonly IDatabaseFactory _databaseFactory;

	public SubItemRepositoryFactory(IDatabaseFactory databaseFactory) => _databaseFactory = databaseFactory;

	public async Task<IRepository<SubItem>> Create()
	{
		var database = await _databaseFactory.Create();
		return new SubItemRepository(database);
	}
}
