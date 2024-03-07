namespace LastEpochSaveEditor.Factories;

public interface ISubItemRepositoryFactory : IRepositoryFactory<SubItem>;

public class SubItemRepositoryFactory(IDatabaseFactory databaseFactory) : ISubItemRepositoryFactory
{
	public async Task<IRepository<SubItem>> Create()
	{
		var database = await databaseFactory.Create();
		return new SubItemRepository(database);
	}
}
